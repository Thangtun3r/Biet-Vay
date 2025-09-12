using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RaceManager : MonoBehaviour
{
    [Header("Horse Layout (Scene Children)")]
    [Tooltip("Parent that contains Horse2D children (one per horse).")]
    public Transform horsesParent;

    [Header("Injected Stats (per horse)")]
    public Vector2 speedRange = new Vector2(6f, 12f);
    public Vector2 staminaRange = new Vector2(60f, 120f);

    [Header("Odds Tuning")]
    [Tooltip("Gamma > 1 widens the spread of odds. 1 = original. e.g., 1.6")]
    public float oddsSharpness = 1.6f;

    [Tooltip("Minimum separation between neighbors in fractional odds (N:1).")]
    public float minFractionalGap = 0.4f;

    [Tooltip("Decimal places for fractional odds label (N:1).")]
    [Range(0, 3)] public int fractionalPrecision = 1;

    [Header("Flow")]
    public float preRaceDelay = 3f;

    [Header("Course")]
    [Tooltip("Drag the finish line Transform here.")]
    public Transform finishLine;

    [Tooltip("Optional: drag a start line Transform. If null, uses startX.")]
    public Transform startLine;

    [Tooltip("If no start line is used, progress start X (local).")]
    public float startX = -8f;

    // runtime list
    private readonly List<Horse2D> horses = new List<Horse2D>();
    public IReadOnlyList<Horse2D> Horses => horses;

    // ── Events ─────────────────────────────────────────────────────────────────
    public event Action<IReadOnlyList<Horse2D>> HorsesSpawned;         // after collect/inject
    public event Action<IReadOnlyList<Horse2D>> HorsesProgressUpdated; // every Update after progress computed
    public event Action RaceStarted;                                   // after enabling horses

    // New: odds event payload (emitted after odds computed)
    public struct OddsEntry
    {
        public int index;            // index in RaceManager.Horses
        public Horse2D horse;        // horse reference
        public float probability;    // 0..1
        public float decimalOdds;    // 1/p (∞ if p=0)
        public string fractional;    // "N:1" label
    }
    public event Action<IReadOnlyList<OddsEntry>> OddsComputed;

    // course caches
    private float startXLocal;
    private float finishXLocal;
    private float courseLengthAbs;
    private float courseDirSign;

    private void Start()
    {
        StartCoroutine(RunRace());
    }

    private IEnumerator RunRace()
    {
        CollectHorsesAndInjectStats();
        InitCourse();
        LogOddsAndPublish();        // keep your odds logic, but now publish an event

        HorsesSpawned?.Invoke(horses);

        yield return new WaitForSeconds(preRaceDelay);

        foreach (var h in horses)
            h.enabled = true;

        RaceStarted?.Invoke();
    }

    private void CollectHorsesAndInjectStats()
    {
        horses.Clear();

        if (horsesParent == null)
        {
            Debug.LogError("[RaceManager] No horsesParent assigned!");
            return;
        }

        for (int i = 0; i < horsesParent.childCount; i++)
        {
            var child = horsesParent.GetChild(i);
            var horse = child.GetComponent<Horse2D>();
            if (horse == null)
            {
                Debug.LogWarning($"[RaceManager] Child '{child.name}' has no Horse2D. Skipping.");
                continue;
            }

            horse.speed   = Random.Range(speedRange.x, speedRange.y);
            horse.stamina = Random.Range(staminaRange.x, staminaRange.y);

            horse.enabled = false; // will be enabled at race start
            horses.Add(horse);
        }
    }

    private void InitCourse()
    {
        startXLocal = (startLine != null)
            ? transform.InverseTransformPoint(startLine.position).x
            : startX;

        if (finishLine != null)
            finishXLocal = transform.InverseTransformPoint(finishLine.position).x;
        else
            Debug.LogWarning("RaceManager: finishLine not set. Progress will not be computed.");

        float delta = finishXLocal - startXLocal;
        courseDirSign = Mathf.Sign(delta == 0f ? 1f : delta);
        courseLengthAbs = Mathf.Max(0.0001f, Mathf.Abs(delta));
    }

    private void Update()
    {
        if (horses.Count == 0 || finishLine == null) return;

        // allow moved lines
        finishXLocal = transform.InverseTransformPoint(finishLine.position).x;
        float delta = finishXLocal - startXLocal;
        courseDirSign = Mathf.Sign(delta == 0f ? 1f : delta);
        courseLengthAbs = Mathf.Max(0.0001f, Mathf.Abs(delta));

        // compute & push progress
        for (int i = 0; i < horses.Count; i++)
        {
            var h = horses[i];
            float hx = h.transform.localPosition.x;        // progress uses horse local X
            float covered = (hx - startXLocal) * courseDirSign;
            float progress = covered / courseLengthAbs;
            h.UpdateProgress(progress);                    // Horse2D exposes this setter.
        }

        // notify subscribers (halfway/debuff handlers, UI, etc.)
        HorsesProgressUpdated?.Invoke(horses);
    }

    // ── Odds (publish event) ──────────────────────────────────────────────────
    private void LogOddsAndPublish()
    {
        if (horses.Count == 0) return;

        // Base scores from speed (can be swapped to any performance metric)
        List<float> rawScores = horses
            .Select(h => Mathf.Max(0.0001f, h.speed))
            .ToList();

        float gamma = Mathf.Max(0.001f, oddsSharpness);
        List<float> sharpScores = rawScores.Select(s => Mathf.Pow(s, gamma)).ToList();

        float sharpSum = sharpScores.Sum();
        if (sharpSum <= 0f) sharpSum = 1f;

        int n = horses.Count;
        float[] prob = new float[n];
        float[] decOdds = new float[n];
        float[] fracN = new float[n];

        for (int i = 0; i < n; i++)
        {
            prob[i] = sharpScores[i] / sharpSum;
            if (prob[i] <= 0f)
            {
                decOdds[i] = float.PositiveInfinity;
                fracN[i]   = float.PositiveInfinity;
            }
            else
            {
                decOdds[i] = 1f / prob[i];   // decimal odds
                fracN[i]   = decOdds[i] - 1f; // fractional "N:1"
            }
        }

        // Enforce minimum gaps on fractional odds in ascending order
        int[] order = Enumerable.Range(0, n).OrderBy(i => fracN[i]).ToArray();
        for (int k = 1; k < order.Length; k++)
        {
            int iPrev = order[k - 1];
            int iCur  = order[k];
            if (float.IsInfinity(fracN[iPrev]) || float.IsInfinity(fracN[iCur])) continue;

            float target = fracN[iPrev] + minFractionalGap;
            if (fracN[iCur] < target) fracN[iCur] = target;
        }

        // Build payload & (optionally) log
        var list = new List<OddsEntry>(n);
        for (int i = 0; i < n; i++)
        {
            string fracLabel = FormatFractionLabel(fracN[i], fractionalPrecision);
            string decLabel  = float.IsInfinity(decOdds[i]) ? "∞" : decOdds[i].ToString("0.00");
            string probLabel = prob[i].ToString("P1");

            // Debug.Log($"Horse[{i}] {horses[i].name} → {fracLabel} (dec {decLabel}, p {probLabel})");

            list.Add(new OddsEntry
            {
                index        = i,
                horse        = horses[i],
                probability  = prob[i],
                decimalOdds  = decOdds[i],
                fractional   = fracLabel
            });
        }

        // Publish to listeners (e.g., HorseRosterAssigner will push to BettingInfoDisplay rows)
        OddsComputed?.Invoke(list);
    }

    private string FormatFractionLabel(float n, int precision)
    {
        if (float.IsInfinity(n) || n < 0f) return "∞:1";
        string fmt = precision <= 0 ? "0" : "0." + new string('#', precision);
        return n.ToString(fmt) + ":1";
    }
}

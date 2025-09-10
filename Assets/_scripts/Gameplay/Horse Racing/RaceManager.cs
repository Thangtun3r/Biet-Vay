using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    [Header("Prefab & Layout")]
    [Tooltip("Horse prefab that has the Horse2D component attached.")]
    public Horse2D horsePrefab;
    public int horseCount = 6;
    public float startX = -8f;
    public float ySpacing = 1.75f;

    [Header("Injected Stats (per horse)")]
    public Vector2 speedRange = new Vector2(6f, 12f);     // min/max speed
    public Vector2 staminaRange = new Vector2(60f, 120f); // min/max stamina (used for racing only; NOT for odds)

    [Header("Odds Tuning")]
    [Tooltip("Gamma > 1 widens the spread of odds. 1 = original. e.g., 1.6")]
    public float oddsSharpness = 1.6f;

    [Tooltip("Minimum separation between neighbors in fractional odds (N:1).")]
    public float minFractionalGap = 0.4f;

    [Tooltip("Decimal places for fractional odds label (N:1).")]
    [Range(0, 3)] public int fractionalPrecision = 1;

    [Header("Flow")]
    public float preRaceDelay = 3f;

    // --- NEW ---
    [Header("Course")]
    [Tooltip("Drag the finish line Transform here (e.g., a sprite or empty).")]
    public Transform finishLine;

    [Tooltip("Optional: drag a start line Transform. If null, uses startX.")]
    public Transform startLine;

    private readonly List<Horse2D> horses = new List<Horse2D>();

    // --- NEW ---
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
        SpawnHorsesAndInjectStats();
        InitCourse();
        LogOddsAndRename();

        yield return new WaitForSeconds(preRaceDelay);

        foreach (var h in horses)
            h.enabled = true;
    }

    private void SpawnHorsesAndInjectStats()
    {
        horses.Clear();

        for (int i = 0; i < horseCount; i++)
        {
            Horse2D horse = Instantiate(horsePrefab, transform);

            horse.transform.localPosition = new Vector3(startX, i * ySpacing, 0f);
            horse.transform.localRotation = Quaternion.identity;

            horse.speed   = Random.Range(speedRange.x, speedRange.y);
            horse.stamina = Random.Range(staminaRange.x, staminaRange.y);

            horse.enabled = false;
            horses.Add(horse);
        }
    }

    // --- NEW ---
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

    // --- NEW ---
    private void Update()
    {
        if (horses.Count == 0 || finishLine == null) return;

        finishXLocal = transform.InverseTransformPoint(finishLine.position).x;
        float delta = finishXLocal - startXLocal;
        courseDirSign = Mathf.Sign(delta == 0f ? 1f : delta);
        courseLengthAbs = Mathf.Max(0.0001f, Mathf.Abs(delta));

        foreach (var h in horses)
        {
            float hx = h.transform.localPosition.x;
            float covered = (hx - startXLocal) * courseDirSign;
            float progress = covered / courseLengthAbs;
            h.UpdateProgress(progress);
        }
    }

    private void LogOddsAndRename()
    {
        // unchanged from your original
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
                fracN[i] = float.PositiveInfinity;
            }
            else
            {
                decOdds[i] = 1f / prob[i];
                fracN[i] = decOdds[i] - 1f;
            }
        }

        int[] order = Enumerable.Range(0, n)
            .OrderBy(i => fracN[i])
            .ToArray();

        for (int k = 1; k < order.Length; k++)
        {
            int iPrev = order[k - 1];
            int iCur  = order[k];

            if (float.IsInfinity(fracN[iPrev]) || float.IsInfinity(fracN[iCur])) continue;

            float target = fracN[iPrev] + minFractionalGap;
            if (fracN[iCur] < target)
                fracN[iCur] = target;
        }

        Debug.Log($"--- Betting Board ({n} horses) ---");
        for (int i = 0; i < n; i++)
        {
            string fracLabel = FormatFractionLabel(fracN[i], fractionalPrecision);
            string decLabel  = float.IsInfinity(decOdds[i]) ? "∞" : decOdds[i].ToString("0.00");
            string probLabel = prob[i].ToString("P1");

            horses[i].name = $"Horse #{i + 1} ({fracLabel})";

            Debug.Log($"Horse #{i + 1}: Speed {horses[i].speed:F1}, " +
                      $"Stamina {horses[i].stamina:F0} → Odds {fracLabel} (≈{decLabel}x, {probLabel})");
        }
        Debug.Log("-----------------------------------------");
    }

    private string FormatFractionLabel(float n, int precision)
    {
        if (float.IsInfinity(n) || n < 0f) return "∞:1";
        string fmt = precision <= 0 ? "0" : "0." + new string('#', precision);
        return n.ToString(fmt) + ":1";
    }
}

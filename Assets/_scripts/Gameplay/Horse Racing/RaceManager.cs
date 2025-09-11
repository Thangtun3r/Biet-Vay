using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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

    // --- Course ---
    [Header("Course")]
    [Tooltip("Drag the finish line Transform here (e.g., a sprite or empty).")]
    public Transform finishLine;

    [Tooltip("Optional: drag a start line Transform. If null, uses startX.")]
    public Transform startLine;

    private readonly List<Horse2D> horses = new List<Horse2D>();
    public IReadOnlyList<Horse2D> Horses => horses;  // expose

    public event Action<IReadOnlyList<Horse2D>> HorsesSpawned;

    private float startXLocal;
    private float finishXLocal;
    private float courseLengthAbs;
    private float courseDirSign;

    // --- Halfway Catch-up (one-time rubber-banding) ---
    [Header("Halfway Catch-up")]
    [Tooltip("Enable the one-time halfway event.")]
    public bool enableHalfwayEvent = true;

    [Range(0f, 1f)]
    [Tooltip("All horses must be at/over this progress for the event to be eligible.")]
    public float halfwayThreshold = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Chance the event fires once when threshold condition is first met.")]
    public float halfwayEventChance = 0.6f;

    [Tooltip("Buff mode for the last horse when event fires.")]
    public SpeedBuffMode halfwayBuffMode = SpeedBuffMode.Multiplier;

    [Tooltip("If Multiplier: 1.35 = +35%. If Additive: ignored.")]
    public float halfwayMultiplier = 1.35f;

    [Tooltip("If Additive: flat speed added. If Multiplier: ignored.")]
    public float halfwayAdditive = 2.0f;

    [Tooltip("How long the special modifier lasts.")]
    public float halfwayDuration = 3f;

    private bool _halfwayRolled = false;

    // --- Leader Debuff (first-to-threshold) ---
    [Header("Leader Debuff (first to reach threshold)")]
    [Tooltip("Enable a one-time chance-based debuff for the first horse that reaches the threshold.")]
    public bool enableLeaderDebuff = true;

    [Range(0f, 1f)]
    [Tooltip("Progress threshold that, once first reached by any horse, may trigger a debuff.")]
    public float leaderDebuffThreshold = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Chance to apply the debuff once the first horse reaches the threshold.")]
    public float leaderDebuffChance = 0.4f;

    [Tooltip("Debuff mode for the leader when event fires.")]
    public SpeedBuffMode leaderDebuffMode = SpeedBuffMode.Multiplier;

    [Tooltip("If Multiplier: <1 slows (e.g., 0.8 = -20%). If Additive: ignored.")]
    public float leaderDebuffMultiplier = 0.8f;

    [Tooltip("If Additive: negative slows (e.g., -2). If Multiplier: ignored.")]
    public float leaderDebuffAdditive = -2.0f;

    [Tooltip("How long the leader debuff lasts.")]
    public float leaderDebuffDuration = 2.5f;

    private bool _leaderDebuffRolled = false;      // ensures one-time roll
    private Horse2D _firstAtLeaderThreshold = null; // who hit the threshold first

    private void Start()
    {
        StartCoroutine(RunRace());
    }

    private IEnumerator RunRace()
    {
        SpawnHorsesAndInjectStats();
        InitCourse();
        LogOddsAndRename();

        // notify visuals that horses exist and are positioned
        HorsesSpawned?.Invoke(horses);

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

            Vector3 spawnPos;

            if (startLine != null)
            {
                // Use startLine world position, offset each horse on Y
                Vector3 basePos = transform.InverseTransformPoint(startLine.position);
                spawnPos = new Vector3(basePos.x, i * ySpacing, 0f);
            }
            else
            {
                // Fall back to startX if no startLine is set
                spawnPos = new Vector3(startX, i * ySpacing, 0f);
            }

            horse.transform.localPosition = spawnPos;
            horse.transform.localRotation = Quaternion.identity;

            horse.speed   = Random.Range(speedRange.x, speedRange.y);
            horse.stamina = Random.Range(staminaRange.x, staminaRange.y);

            horse.enabled = false;
            horses.Add(horse);
        }

        // reset one-time flags for a new race
        _halfwayRolled = false;
        _leaderDebuffRolled = false;
        _firstAtLeaderThreshold = null;
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

        // Recompute in case finish line moves
        finishXLocal = transform.InverseTransformPoint(finishLine.position).x;
        float delta = finishXLocal - startXLocal;
        courseDirSign = Mathf.Sign(delta == 0f ? 1f : delta);
        courseLengthAbs = Mathf.Max(0.0001f, Mathf.Abs(delta));

        // Update progress for each horse
        for (int i = 0; i < horses.Count; i++)
        {
            var h = horses[i];
            float hx = h.transform.localPosition.x;
            float covered = (hx - startXLocal) * courseDirSign;
            float progress = covered / courseLengthAbs;
            h.UpdateProgress(progress);
        }

        // One-time events
        CheckHalfwayCatchup();
        CheckLeaderDebuff();
    }

    private void CheckHalfwayCatchup()
    {
        if (!enableHalfwayEvent || _halfwayRolled || horses.Count == 0) return;

        // Find last place & min progress
        float minP = 1f;
        int lastIdx = -1;

        for (int i = 0; i < horses.Count; i++)
        {
            float p = horses[i].progress01;
            if (p < minP) { minP = p; lastIdx = i; }
        }

        // Fire only after EVERY horse has reached the threshold (min >= threshold)
        if (minP >= halfwayThreshold)
        {
            _halfwayRolled = true; // roll only once per race

            if (Random.value <= halfwayEventChance && lastIdx >= 0)
            {
                var lastHorse = horses[lastIdx];
                var sm = lastHorse.speedManager;
                if (sm != null)
                {
                    if (halfwayBuffMode == SpeedBuffMode.Multiplier)
                    {
                        sm.TriggerTimedMultiplier(halfwayDuration, halfwayMultiplier);
                        Debug.Log($"[Halfway] {lastHorse.name} gets x{halfwayMultiplier:0.##} for {halfwayDuration:0.##}s.");
                    }
                    else
                    {
                        sm.TriggerTimedAdditive(halfwayDuration, halfwayAdditive);
                        Debug.Log($"[Halfway] {lastHorse.name} gets +{halfwayAdditive:0.##} for {halfwayDuration:0.##}s.");
                    }
                }
                else
                {
                    Debug.LogWarning("[Halfway] Last horse has no SpeedAffectoManager; cannot apply catch-up.");
                }
            }
            else
            {
                Debug.Log("[Halfway] Catch-up roll failed or no valid last horse. No buff this race.");
            }
        }
    }

    private void CheckLeaderDebuff()
    {
        if (!enableLeaderDebuff || _leaderDebuffRolled || horses.Count == 0) return;

        // If we haven't locked in who reached the threshold first yet, check if any horse crossed it.
        if (_firstAtLeaderThreshold == null)
        {
            Horse2D candidate = null;
            float bestProg = leaderDebuffThreshold;

            // Pick the first frame a horse crosses the threshold.
            // If multiple cross in the same frame, choose the one with the highest progress (furthest ahead).
            for (int i = 0; i < horses.Count; i++)
            {
                var h = horses[i];
                if (h.progress01 >= leaderDebuffThreshold && h.progress01 >= bestProg)
                {
                    if (candidate == null || h.progress01 > bestProg)
                    {
                        candidate = h;
                        bestProg = h.progress01;
                    }
                }
            }

            if (candidate != null)
            {
                _firstAtLeaderThreshold = candidate;
                _leaderDebuffRolled = true; // one-time roll

                if (Random.value <= leaderDebuffChance)
                {
                    var sm = candidate.speedManager;
                    if (sm != null)
                    {
                        if (leaderDebuffMode == SpeedBuffMode.Multiplier)
                        {
                            sm.TriggerTimedMultiplier(leaderDebuffDuration, leaderDebuffMultiplier);
                            Debug.Log($"[LeaderDebuff] {candidate.name} slowed x{leaderDebuffMultiplier:0.##} for {leaderDebuffDuration:0.##}s.");
                        }
                        else
                        {
                            sm.TriggerTimedAdditive(leaderDebuffDuration, leaderDebuffAdditive);
                            Debug.Log($"[LeaderDebuff] {candidate.name} slowed {leaderDebuffAdditive:+0.##;-0.##} for {leaderDebuffDuration:0.##}s.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[LeaderDebuff] Leader has no SpeedAffectoManager; cannot apply debuff.");
                    }
                }
                else
                {
                    Debug.Log("[LeaderDebuff] Roll failed. No debuff this race.");
                }
            }
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

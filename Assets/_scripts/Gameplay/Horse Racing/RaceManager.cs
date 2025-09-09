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

    private readonly List<Horse2D> horses = new List<Horse2D>();

    private void Start()
    {
        StartCoroutine(RunRace());
    }

    private IEnumerator RunRace()
    {
        SpawnHorsesAndInjectStats();
        LogOddsAndRename();

        // Let players read the board
        yield return new WaitForSeconds(preRaceDelay);

        // Go! (enable movement by enabling the component)
        foreach (var h in horses)
            h.enabled = true;
    }

    private void SpawnHorsesAndInjectStats()
    {
        horses.Clear();

        for (int i = 0; i < horseCount; i++)
        {
            // Instantiate as a CHILD of RaceManager, then place using localPosition
            Horse2D horse = Instantiate(horsePrefab, transform);

            // Local layout: startX on local X, stacked on local Y
            horse.transform.localPosition = new Vector3(startX, i * ySpacing, 0f);
            horse.transform.localRotation = Quaternion.identity;

            // Inject stats
            horse.speed   = Random.Range(speedRange.x, speedRange.y);
            horse.stamina = Random.Range(staminaRange.x, staminaRange.y);

            // Keep them still until the start
            horse.enabled = false;

            horses.Add(horse);
        }
    }

    private void LogOddsAndRename()
    {
        // 1) Raw scores based on SPEED ONLY so higher speed => higher probability => LOWER odds
        List<float> rawScores = horses
            .Select(h => Mathf.Max(0.0001f, h.speed))
            .ToList();

        // 2) Sharpen to widen variance: p_i ∝ (score_i)^gamma
        float gamma = Mathf.Max(0.001f, oddsSharpness);
        List<float> sharpScores = rawScores.Select(s => Mathf.Pow(s, gamma)).ToList();

        float sharpSum = sharpScores.Sum();
        if (sharpSum <= 0f) sharpSum = 1f;

        // 3) Implied probabilities & decimal odds
        int n = horses.Count;
        float[] prob = new float[n];
        float[] decOdds = new float[n];     // decimal: 1/prob
        float[] fracN = new float[n];       // fractional N in "N:1" (profit per 1 stake)

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
                fracN[i] = decOdds[i] - 1f; // fractional N:1
            }
        }

        // 4) Enforce a minimum gap on fractional odds so labels aren't too close.
        // Sort by favorability (favorites have SMALLER N:1), then push neighbors apart.
        int[] order = Enumerable.Range(0, n)
            .OrderBy(i => fracN[i]) // ascending N:1
            .ToArray();

        for (int k = 1; k < order.Length; k++)
        {
            int iPrev = order[k - 1];
            int iCur  = order[k];

            // Only adjust finite values
            if (float.IsInfinity(fracN[iPrev]) || float.IsInfinity(fracN[iCur])) continue;

            float target = fracN[iPrev] + minFractionalGap;
            if (fracN[iCur] < target)
                fracN[iCur] = target;
        }

        // 5) Display + rename using DECIMAL fractional (no integer rounding)
        Debug.Log($"--- Betting Board ({n} horses) ---");
        for (int i = 0; i < n; i++)
        {
            string fracLabel = FormatFractionLabel(fracN[i], fractionalPrecision);
            string decLabel  = float.IsInfinity(decOdds[i]) ? "∞" : decOdds[i].ToString("0.00");
            string probLabel = prob[i].ToString("P1");

            // Rename horse to include fractional odds label (e.g., "2.5:1")
            horses[i].name = $"Horse #{i + 1} ({fracLabel})";

            Debug.Log($"Horse #{i + 1}: Speed {horses[i].speed:F1}, " +
                      $"Stamina {horses[i].stamina:F0} → Odds {fracLabel} (≈{decLabel}x, {probLabel})");
        }
        Debug.Log("-----------------------------------------");
    }

    private string FormatFractionLabel(float n, int precision)
    {
        if (float.IsInfinity(n) || n < 0f) return "∞:1";
        // e.g., precision=1 -> "0.#", precision=2 -> "0.##"
        string fmt = precision <= 0 ? "0" : "0." + new string('#', precision);
        return n.ToString(fmt) + ":1";
    }
}

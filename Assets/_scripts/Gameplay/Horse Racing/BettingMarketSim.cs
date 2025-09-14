using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BettingMarketSimulator : MonoBehaviour
{
    [Header("Scene Refs")]
    public RaceManager raceManager;
    public Transform horseInfoContentParent;
    public TMP_Text totalPoolSizeText;

    [Header("Initial Pool (per horse)")]
    public long initialMin = 120_000_000;
    public long initialMax = 180_000_000;

    [Header("Pool Growth (Targets)")]
    [Tooltip("How often we pick NEW targets (seconds). Only some rows will change each tick.")]
    public float poolTargetInterval = 0.9f;

    [Tooltip("Per tick, the chance that a given row will get a new (higher) target.")]
    [Range(0f,1f)] public float poolUpdateChance = 0.55f;

    [Tooltip("When a row is picked, add a random amount in this range (inclusive).")]
    public long poolTickMin = 120_000;
    public long poolTickMax = 700_000;

    [Header("Pool Animation (Lerp)")]
    [Tooltip("How quickly displayed pool moves toward target (units per second).")]
    public float poolLerpPerSecond = 2_000_000f;

    [Header("Odds Wobble (very small)")]
    [Tooltip("Seconds between odds target checks; only some rows will wobble each tick.")]
    public float oddsJitterInterval = 1.1f;

    [Tooltip("Per tick, the chance that a given row will get a tiny odds wobble.")]
    [Range(0f,1f)] public float oddsUpdateChance = 0.45f;

    [Tooltip("Max absolute jitter applied to fractional N (e.g., 0.03 = ±0.03).")]
    public float oddsJitterRange = 0.03f;

    [Tooltip("How fast displayed fractional N moves toward its target per second.")]
    public float oddsLerpPerSecond = 0.15f;

    [Tooltip("Clamp fractional N within a safe range for display.")]
    public Vector2 fractionalClampN = new Vector2(1.0f, 99.0f);

    // --- state ---
    private readonly List<BettingInfoDisplay> _rows = new List<BettingInfoDisplay>();
    private readonly List<long>   _poolTarget = new List<long>();   // desired target
    private readonly List<double> _poolShown  = new List<double>(); // lerped display
    private readonly List<float>  _baseFracN  = new List<float>();  // RM-provided base
    private readonly List<float>  _oddsTargetN = new List<float>(); // tiny jitter target
    private readonly List<float>  _oddsShownN  = new List<float>(); // lerped display

    private Coroutine _poolChooserLoop;
    private Coroutine _oddsJitterLoop;
    private bool _frozen;

    // field-relative odds range (updated each time odds are computed)
    private float _fieldMinN = 1f;
    private float _fieldMaxN = 1f;

    void OnEnable()
    {
        if (raceManager == null)
            raceManager = GetComponent<RaceManager>();

        if (raceManager != null)
        {
            raceManager.HorsesSpawned += OnHorsesSpawned;
            raceManager.OddsComputed  += OnOddsComputed;
            raceManager.RaceStarted   += OnRaceStarted;
        }
        CollectRows();
    }

    void OnDisable()
    {
        if (raceManager != null)
        {
            raceManager.HorsesSpawned -= OnHorsesSpawned;
            raceManager.OddsComputed  -= OnOddsComputed;
            raceManager.RaceStarted   -= OnRaceStarted;
        }
        if (_poolChooserLoop != null) StopCoroutine(_poolChooserLoop);
        if (_oddsJitterLoop != null) StopCoroutine(_oddsJitterLoop);
    }

    private void CollectRows()
    {
        _rows.Clear();
        if (horseInfoContentParent == null) return;

        for (int i = 0; i < horseInfoContentParent.childCount; i++)
        {
            var row = horseInfoContentParent.GetChild(i).GetComponent<BettingInfoDisplay>();
            if (row != null) _rows.Add(row);
        }
    }

    private void OnHorsesSpawned(IReadOnlyList<Horse2D> _)
    {
        _frozen = false;

        _poolTarget.Clear();
        _poolShown.Clear();
        for (int i = 0; i < _rows.Count; i++)
        {
            long seed = RandomLong(initialMin, initialMax);

            // if we already have odds (e.g. from a prior compute), scale seed now
            if (_oddsShownN.Count > i)
                seed = (long)System.Math.Round(seed * OddsScale(_oddsShownN[i]));

            _poolTarget.Add(seed);
            _poolShown.Add(seed);
            _rows[i].SetPerHorsePool(seed);
        }
        UpdateTotalLabel();

        if (_poolChooserLoop != null) StopCoroutine(_poolChooserLoop);
        _poolChooserLoop = StartCoroutine(PickNewPoolTargetsLoop());
    }

    private void OnOddsComputed(IReadOnlyList<RaceManager.OddsEntry> odds)
    {
        _baseFracN.Clear();
        _oddsTargetN.Clear();
        _oddsShownN.Clear();

        _fieldMinN = float.PositiveInfinity;
        _fieldMaxN = float.NegativeInfinity;

        for (int i = 0; i < _rows.Count && i < odds.Count; i++)
        {
            float n = ParseFractionalN(odds[i].fractional);
            n = Mathf.Clamp(n, fractionalClampN.x, fractionalClampN.y);

            _fieldMinN = Mathf.Min(_fieldMinN, n);
            _fieldMaxN = Mathf.Max(_fieldMaxN, n);

            _baseFracN.Add(n);
            _oddsTargetN.Add(n);
            _oddsShownN.Add(n);
            _rows[i].SetPayoutOdds(n.ToString("0.#") + ":1");
        }

        // softly rebase pool targets toward field-relative scaled levels
        for (int i = 0; i < _poolTarget.Count && i < _oddsShownN.Count; i++)
        {
            float scale = OddsScale(_oddsShownN[i]);
            double rebased = _poolShown[i] * scale;
            _poolTarget[i] = (long)System.Math.Round(
                Mathf.Lerp(_poolTarget[i], (float)rebased, 0.25f) // 25% toward new level
            );
        }

        if (_oddsJitterLoop != null) StopCoroutine(_oddsJitterLoop);
        _oddsJitterLoop = StartCoroutine(PickNewOddsTargetsLoop());
    }

    private void OnRaceStarted()
    {
        _frozen = true;

        if (_poolChooserLoop != null) StopCoroutine(_poolChooserLoop);
        if (_oddsJitterLoop != null) StopCoroutine(_oddsJitterLoop);

        // Snap pools: shown == target, so no post-freeze lerp creep
        for (int i = 0; i < _poolTarget.Count && i < _poolShown.Count; i++)
        {
            long snap = (long)System.Math.Round(_poolShown[i]);
            _poolTarget[i] = snap;
            _poolShown[i]  = snap;
            if (i < _rows.Count) _rows[i].SetPerHorsePool(snap);
        }
        UpdateTotalLabel();

        // Snap odds too (optional)
        for (int i = 0; i < _oddsTargetN.Count && i < _oddsShownN.Count; i++)
            _oddsTargetN[i] = _oddsShownN[i];
    }


    // ── Target selection loops (only some rows each tick) ─────────────────────
    private IEnumerator PickNewPoolTargetsLoop()
    {
        var wait = new WaitForSeconds(poolTargetInterval);
        while (!_frozen)
        {
            for (int i = 0; i < _poolTarget.Count; i++)
            {
                if (Random.value <= poolUpdateChance)
                {
                    long inc = RandomLong(poolTickMin, poolTickMax);

                    // prefer current shown odds; fall back to base if needed
                    float n = (i < _oddsShownN.Count) ? _oddsShownN[i]
                             : (i < _baseFracN.Count) ? _baseFracN[i]
                             : 1f;

                    float scale = OddsScale(n);
                    inc = (long)System.Math.Round(inc * scale);

                    _poolTarget[i] = _poolTarget[i] + inc; // only increases
                }
            }
            yield return wait;
        }
    }

    private IEnumerator PickNewOddsTargetsLoop()
    {
        var wait = new WaitForSeconds(oddsJitterInterval);
        while (!_frozen)
        {
            for (int i = 0; i < _oddsTargetN.Count && i < _baseFracN.Count; i++)
            {
                if (Random.value <= oddsUpdateChance)
                {
                    float baseN = _baseFracN[i];
                    float delta = Random.Range(-oddsJitterRange, oddsJitterRange);
                    float tgt = Mathf.Clamp(baseN + delta, fractionalClampN.x, fractionalClampN.y);
                    _oddsTargetN[i] = tgt;
                }
            }
            yield return wait;
        }
    }

    // ── Per-frame smoothing ───────────────────────────────────────────────────
    void Update()
    {
        if (_rows.Count == 0) return;
        if (_frozen) return; // hard lock during race

        // Smoothly move displayed pools toward targets
        double total = 0;
        for (int i = 0; i < _rows.Count && i < _poolTarget.Count && i < _poolShown.Count; i++)
        {
            double current = _poolShown[i];
            double target  = _poolTarget[i];

            double step = poolLerpPerSecond * Time.deltaTime;
            double next = MoveTowardsDouble(current, target, step);

            _poolShown[i] = next;
            long shownLong = (long)System.Math.Round(next);
            _rows[i].SetPerHorsePool(shownLong);

            total += next;
        }
        UpdateTotalLabel((long)System.Math.Round(total));

        // Smoothly move displayed odds toward (rarely-updated) small jitter targets
        for (int i = 0; i < _rows.Count && i < _oddsTargetN.Count && i < _oddsShownN.Count; i++)
        {
            float cur = _oddsShownN[i];
            float tgt = _oddsTargetN[i];
            float step = oddsLerpPerSecond * Time.deltaTime;

            float next = Mathf.MoveTowards(cur, tgt, step);
            _oddsShownN[i] = next;

            _rows[i].SetPayoutOdds(next.ToString("0.#") + ":1");
        }
    }

    // ── TOTAL POOL LABEL ──────────────────────────────────────────────────────
    private void UpdateTotalLabel()
    {
        double total = 0;
        for (int i = 0; i < _poolShown.Count; i++)
            total += _poolShown[i];

        if (totalPoolSizeText != null)
            totalPoolSizeText.text = ((long)System.Math.Round(total)).ToString("N0");
    }

    private void UpdateTotalLabel(long total)
    {
        if (totalPoolSizeText != null)
            totalPoolSizeText.text = total.ToString("N0");
    }

    // ── helpers ───────────────────────────────────────────────────────────────
    private static long RandomLong(long minInclusive, long maxInclusive)
    {
        if (maxInclusive < minInclusive) (minInclusive, maxInclusive) = (maxInclusive, minInclusive);
        long range = maxInclusive - minInclusive;
        if (range <= int.MaxValue)
            return minInclusive + Random.Range(0, (int)range + 1);

        int hi = Random.Range(0, 1 << 16);
        int lo = Random.Range(0, 1 << 16);
        long rnd = ((long)hi << 16) | (uint)lo;
        return minInclusive + (rnd % (range + 1));
    }

    private static double MoveTowardsDouble(double current, double target, double maxDelta)
    {
        if (System.Math.Abs(target - current) <= maxDelta)
            return target;
        return current + System.Math.Sign(target - current) * maxDelta;
    }

    private static float ParseFractionalN(string fractionalLabel)
    {
        if (string.IsNullOrEmpty(fractionalLabel)) return 1f;
        int idx = fractionalLabel.IndexOf(':');
        if (idx <= 0) return 1f;
        string left = fractionalLabel.Substring(0, idx);
        if (float.TryParse(left, out float n)) return n;
        return 1f;
    }

    // favorites (low N) -> larger pools; longshots (high N) -> smaller pools
    // field-relative mapping: N in [fieldMin, fieldMax] -> scale in [maxScale, minScale]
    private float OddsScale(float n)
    {
        const float maxScale = 1.8f; // how much to boost favorites
        const float minScale = 0.6f; // how much to reduce longshots

        // fallback if field range not ready yet
        float minN = (_fieldMinN < _fieldMaxN) ? _fieldMinN : fractionalClampN.x;
        float maxN = (_fieldMinN < _fieldMaxN) ? _fieldMaxN : fractionalClampN.y;

        n = Mathf.Clamp(n, minN, maxN);
        float t = (n - minN) / Mathf.Max(0.0001f, (maxN - minN)); // 0=favorite, 1=longshot
        return Mathf.Lerp(maxScale, minScale, t);
    }
}

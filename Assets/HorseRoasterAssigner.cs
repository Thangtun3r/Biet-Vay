using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class HorseRosterAssigner : MonoBehaviour
{
    [Serializable]
    public struct HorseIdentity
    {
        public string displayName;
        public Color color;
    }

    [Header("Source Pool (Name ↔ Color)")]
    public HorseIdentity[] rosterPool;

    [Header("Randomization")]
    public int randomSeed = -1;

    [Tooltip("If there are fewer entries than horses, reuse after pool exhausts.")]
    public bool allowReuseIfPoolTooSmall = true;

    [Header("Apply Options")]
    public bool renameVisualGameObject = true;

    [Header("Betting UI (Optional)")]
    [Tooltip("Parent that contains child rows. Each child should have a BettingInfoDisplay.")]
    public Transform horseInfoContentParent;

    private RaceManager _raceManager;

    // Keep mapping from horse to assigned identity so we can re-use when odds arrive
    private readonly Dictionary<Horse2D, HorseIdentity> _assigned = new Dictionary<Horse2D, HorseIdentity>();
    private readonly List<Horse2D> _lastHorses = new List<Horse2D>();

    void Awake()
    {
        _raceManager = GetComponent<RaceManager>();
        if (_raceManager == null)
            Debug.LogError("[HorseRosterAssigner] RaceManager not found on the same GameObject.");
    }

    void OnEnable()
    {
        if (_raceManager != null)
        {
            _raceManager.HorsesSpawned += OnHorsesSpawned;
            _raceManager.OddsComputed += OnOddsComputed; // <— listen to odds
        }
    }

    void OnDisable()
    {
        if (_raceManager != null)
        {
            _raceManager.HorsesSpawned -= OnHorsesSpawned;
            _raceManager.OddsComputed -= OnOddsComputed;
        }
    }

    private void OnHorsesSpawned(IReadOnlyList<Horse2D> horses)
    {
        _assigned.Clear();
        _lastHorses.Clear();

        if (horses == null || horses.Count == 0) return;

        var pool = (rosterPool ?? Array.Empty<HorseIdentity>()).ToList();
        if (pool.Count == 0)
        {
            Debug.LogWarning("[HorseRosterAssigner] Roster pool is empty. Skipping assignment.");
            return;
        }

        System.Random rng = (randomSeed >= 0) ? new System.Random(randomSeed) : new System.Random();
        List<HorseIdentity> bag = new List<HorseIdentity>(pool);
        Shuffle(bag, rng);

        int bagIndex = 0;
        for (int i = 0; i < horses.Count; i++)
        {
            if (bagIndex >= bag.Count)
            {
                if (!allowReuseIfPoolTooSmall)
                {
                    Debug.LogWarning("[HorseRosterAssigner] Not enough unique entries; stopping assignment.");
                    break;
                }
                bagIndex = 0;
                Shuffle(bag, rng);
            }

            var id = bag[bagIndex++];
            var horse = horses[i];

            // Apply visuals (existing behavior)
            var visual = horse.GetComponentInChildren<HorseVisual>();
            if (visual != null)
            {
                string chosenName = string.IsNullOrWhiteSpace(id.displayName) ? $"Horse #{i + 1}" : id.displayName.Trim();
                visual.ApplyDisplayName(chosenName, renameVisualGameObject);
                visual.ApplyColor(id.color);

                // Remember mapping for odds UI later
                _assigned[horse] = new HorseIdentity { displayName = chosenName, color = id.color };
            }
            else
            {
                Debug.LogWarning($"[HorseRosterAssigner] No HorseVisual found under {horse.name}.");
            }

            _lastHorses.Add(horse);
        }

        // Optionally initialize the betting rows with just name/color (odds may arrive shortly)
        TryFillBettingRowsInitial();
    }

    private void OnOddsComputed(IReadOnlyList<RaceManager.OddsEntry> entries)
    {
        if (entries == null || entries.Count == 0) return;

        // Fill/refresh betting rows with name, color, and odds
        TryFillBettingRowsWithOdds(entries);
    }

    // ---------------- UI helpers ----------------

    private void TryFillBettingRowsInitial()
    {
        if (horseInfoContentParent == null) return;

        int rowCount = horseInfoContentParent.childCount;
        int n = Mathf.Min(_lastHorses.Count, rowCount);

        for (int i = 0; i < n; i++)
        {
            var row = horseInfoContentParent.GetChild(i);
            var ui = row.GetComponent<BettingInfoDisplay>();
            if (ui == null) continue; // skip rows without the component

            var horse = _lastHorses[i];
            if (_assigned.TryGetValue(horse, out var id))
            {
                ui.SetHorseName(id.displayName);
                ui.SetSilksColor(id.color);
                // odds not known yet; leave payout empty
            }
        }
    }

    private void TryFillBettingRowsWithOdds(IReadOnlyList<RaceManager.OddsEntry> entries)
    {
        if (horseInfoContentParent == null) return;

        int rowCount = horseInfoContentParent.childCount;
        int n = Mathf.Min(entries.Count, rowCount);

        for (int i = 0; i < n; i++)
        {
            var row = horseInfoContentParent.GetChild(i);
            var ui = row.GetComponent<BettingInfoDisplay>();
            if (ui == null) continue;

            // We prefer to use the same order as RaceManager odds entries
            var e = entries[i];
            var horse = e.horse;

            // Lookup the identity we assigned earlier
            if (_assigned.TryGetValue(horse, out var id))
            {
                ui.SetHorseName(id.displayName);
                ui.SetSilksColor(id.color);

                // Use fractional odds label from RaceManager, e.g., "3.2:1"
                ui.SetPayoutOdds(e.fractional);

                // (Optional) If you track per-horse pool elsewhere, set it here.
                // ui.SetPerHorsePool(poolAmount);
            }
            else
            {
                // Fallback if somehow no mapping exists
                ui.SetHorseName($"Horse #{i + 1}");
                ui.SetPayoutOdds(e.fractional);
            }
        }
    }

    private static void Shuffle<T>(IList<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}

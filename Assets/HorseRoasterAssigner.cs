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

    private RaceManager _raceManager;

    void Awake()
    {
        _raceManager = GetComponent<RaceManager>();
        if (_raceManager == null)
            Debug.LogError("[HorseRosterAssigner] RaceManager not found on the same GameObject.");
    }

    void OnEnable()
    {
        if (_raceManager != null)
            _raceManager.HorsesSpawned += OnHorsesSpawned;
    }

    void OnDisable()
    {
        if (_raceManager != null)
            _raceManager.HorsesSpawned -= OnHorsesSpawned;
    }

    private void OnHorsesSpawned(IReadOnlyList<Horse2D> horses)
    {
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

            var visual = horse.GetComponentInChildren<HorseVisual>();
            if (visual != null)
            {
                visual.ApplyDisplayName(
                    string.IsNullOrWhiteSpace(id.displayName) ? $"Horse #{i + 1}" : id.displayName.Trim(),
                    renameVisualGameObject);

                visual.ApplyColor(id.color);
            }
            else
            {
                Debug.LogWarning($"[HorseRosterAssigner] No HorseVisual found under {horse.name}.");
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

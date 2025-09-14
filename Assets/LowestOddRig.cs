using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// On odds computation, rig the lowest-odds (favorite) horse with
/// a chance to get a speed buff at the start of the race.
/// </summary>
[DisallowMultipleComponent]
public class LowestOddsRigHandler : MonoBehaviour
{
    [Header("Enable Rigging")]
    public bool enabledRig = true;

    [Header("Buff Settings")]
    [Tooltip("Chance (0-1) that the lowest-odds horse will get the buff.")]
    [Range(0f, 1f)] public float chance = 0.7f;

    [Tooltip("If Multiplier: 1.2 = +20% speed.")]
    public float multiplier = 1.2f;

    [Tooltip("How long the buff lasts (seconds). Use <=0 for permanent.")]
    public float duration = 5f;

    private RaceManager _rm;
    private bool _rolled;

    private void Awake()
    {
        _rm = GetComponent<RaceManager>();
        if (_rm == null)
            Debug.LogError("[LowestOddsRigHandler] RaceManager not found.");
    }

    private void OnEnable()
    {
        if (_rm != null) _rm.OddsComputed += OnOddsComputed;
    }

    private void OnDisable()
    {
        if (_rm != null) _rm.OddsComputed -= OnOddsComputed;
        _rolled = false;
    }

    private void OnOddsComputed(IReadOnlyList<RaceManager.OddsEntry> odds)
    {
        if (!enabledRig || _rolled || odds == null || odds.Count == 0)
            return;

        // Find horse with lowest decimal odds (the favorite)
        RaceManager.OddsEntry best = odds[0];
        for (int i = 1; i < odds.Count; i++)
        {
            if (odds[i].decimalOdds < best.decimalOdds)
                best = odds[i];
        }

        _rolled = true;
        if (Random.value <= chance)
        {
            var sm = best.horse.speedManager;
            if (sm != null)
            {
                sm.TriggerTimedMultiplier(duration, multiplier);
                Debug.Log($"[Rig] {best.horse.name} gets x{multiplier:0.##} for {duration:0.##}s (lowest odds buff).");
            }
            else
            {
                Debug.LogWarning("[Rig] Favorite horse has no SpeedAffectoManager; cannot apply buff.");
            }
        }
        else
        {
            Debug.Log("[Rig] Roll failed. No buff applied to lowest odds horse.");
        }
    }
}

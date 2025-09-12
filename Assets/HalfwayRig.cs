using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Subscribes to RaceManager.HorsesProgressUpdated and applies a one-time
/// catch-up buff to the last horse when every horse reaches a threshold.
/// </summary>
[DisallowMultipleComponent]
public class HalfwayCatchupHandler : MonoBehaviour
{
    [Header("Enable")]
    public bool enabledHalfwayEvent = true;

    [Header("Trigger")]
    [Range(0f, 1f)]
    [Tooltip("All horses must be at/over this progress for the event to be eligible.")]
    public float threshold = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Chance the event fires once when condition is first met.")]
    public float chance = 0.6f;

    [Header("Effect")]
    public SpeedBuffMode mode = SpeedBuffMode.Multiplier;
    [Tooltip("If Multiplier: 1.35 = +35%. If Additive: ignored.")]
    public float multiplier = 1.35f;
    [Tooltip("If Additive: flat speed added. If Multiplier: ignored.")]
    public float additive = 2.0f;
    [Tooltip("How long the modifier lasts (seconds).")]
    public float duration = 3f;

    private RaceManager _rm;
    private bool _rolled;

    private void Awake()
    {
        _rm = GetComponent<RaceManager>();
        if (_rm == null) Debug.LogError("[HalfwayCatchupHandler] RaceManager not found.");
    }

    private void OnEnable()
    {
        if (_rm != null) _rm.HorsesProgressUpdated += OnProgress;
    }

    private void OnDisable()
    {
        if (_rm != null) _rm.HorsesProgressUpdated -= OnProgress;
        _rolled = false;
    }

    private void OnProgress(IReadOnlyList<Horse2D> horses)
    {
        if (!enabledHalfwayEvent || _rolled || horses == null || horses.Count == 0) return;

        // find min progress & last index
        float minP = 1f;
        int lastIdx = -1;
        for (int i = 0; i < horses.Count; i++)
        {
            float p = horses[i].progress01;
            if (p < minP) { minP = p; lastIdx = i; }
        }

        // fire only after EVERY horse reached the threshold
        if (minP >= threshold)
        {
            _rolled = true;
            if (Random.value <= chance && lastIdx >= 0)
            {
                var lastHorse = horses[lastIdx];
                var sm = lastHorse.speedManager;
                if (sm != null)
                {
                    if (mode == SpeedBuffMode.Multiplier)
                    {
                        sm.TriggerTimedMultiplier(duration, multiplier);
                        Debug.Log($"[Halfway] {lastHorse.name} gets x{multiplier:0.##} for {duration:0.##}s.");
                    }
                    else
                    {
                        sm.TriggerTimedAdditive(duration, additive);
                        Debug.Log($"[Halfway] {lastHorse.name} gets +{additive:0.##} for {duration:0.##}s.");
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
}

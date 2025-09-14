using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Subscribes to RaceManager.HorsesProgressUpdated and applies a one-time
/// debuff to the first horse that reaches a threshold (chance-based).
/// </summary>
[DisallowMultipleComponent]
public class LeaderDebuffHandler : MonoBehaviour
{
    [Header("Enable")]
    public bool enabledLeaderDebuff = true;

    [Header("Trigger")]
    [Range(0f, 1f)]
    [Tooltip("Progress threshold that, once first reached by any horse, may trigger a debuff.")]
    public float threshold = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Chance to apply the debuff once the first horse reaches the threshold.")]
    public float chance = 0.4f;

    [Header("Effect")]
    public SpeedBuffMode mode = SpeedBuffMode.Multiplier;
    [Tooltip("If Multiplier: <1 slows (e.g., 0.8 = -20%). If Additive: ignored.")]
    public float multiplier = 0.8f;
    [Tooltip("If Additive: negative slows (e.g., -2). If Multiplier: ignored.")]
    public float additive = -2.0f;
    [Tooltip("How long the debuff lasts (seconds).")]
    public float duration = 2.5f;

    private RaceManager _rm;
    private bool _rolled;                 // ensures one-time roll
    private Horse2D _firstAtThreshold;    // who hit the threshold first

    private void Awake()
    {
        _rm = GetComponent<RaceManager>();
        if (_rm == null) Debug.LogError("[LeaderDebuffHandler] RaceManager not found.");
    }

    private void OnEnable()
    {
        if (_rm != null) _rm.HorsesProgressUpdated += OnProgress;
    }

    private void OnDisable()
    {
        if (_rm != null) _rm.HorsesProgressUpdated -= OnProgress;
        _rolled = false;
        _firstAtThreshold = null;
    }

    private void OnProgress(IReadOnlyList<Horse2D> horses)
    {
        if (!enabledLeaderDebuff || _rolled || horses == null || horses.Count == 0) return;

        if (_firstAtThreshold == null)
        {
            Horse2D candidate = null;
            float bestProg = threshold;

            // pick the first frame any crosses; if multiple, choose furthest ahead
            for (int i = 0; i < horses.Count; i++)
            {
                var h = horses[i];
                if (h.progress01 >= threshold && h.progress01 >= bestProg)
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
                _firstAtThreshold = candidate;
                _rolled = true;

                if (Random.value <= chance)
                {
                    var sm = candidate.speedManager;
                    if (sm != null)
                    {
                        if (mode == SpeedBuffMode.Multiplier)
                        {
                            sm.TriggerTimedMultiplier(duration, multiplier);
                            Debug.Log($"[LeaderDebuff] {candidate.name} slowed x{multiplier:0.##} for {duration:0.##}s.");
                        }
                        else
                        {
                            sm.TriggerTimedAdditive(duration, additive);
                            Debug.Log($"[LeaderDebuff] {candidate.name} slowed {additive:+0.##;-0.##} for {duration:0.##}s.");
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
}

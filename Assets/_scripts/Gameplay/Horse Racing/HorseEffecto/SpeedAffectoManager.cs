using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public enum SpeedBuffMode { Multiplier, Additive }

[Serializable]
public class AutoSpeedBuff
{
    public string name = "SuperSpeed";
    public bool enabled = true;

    [Tooltip("Random chance per second to trigger this buff.")]
    public float chancePerSecond = 0.3f;

    [Tooltip("Seconds the buff lasts.")]
    public float duration = 3f;

    [Tooltip("If mode = Multiplier, 1.5 = +50%; if Additive, value added directly to speed.")]
    public float value = 1.5f;

    public SpeedBuffMode mode = SpeedBuffMode.Multiplier;

    [Tooltip("Optional cooldown between triggers of this entry.")]
    public float cooldown = 0f;

    [NonSerialized] public float nextAllowedTime = 0f; // runtime
}

public class SpeedAffectoManager : MonoBehaviour
{
    [Header("Auto-Trigger Buffs")]
    [SerializeField] private bool autoTrigger = true;
    [SerializeField] private List<AutoSpeedBuff> autoBuffs = new();

    // Active runtime modifiers
    private readonly List<ISpeedModifier> mods = new();

    // --- Public API (manual add) ---
    public void AddModifier(ISpeedModifier mod) => mods.Add(mod);

    // --- Public API (manual trigger helper) ---
    public void TriggerTimedMultiplier(float duration, float multiplier)
        => AddModifier(new TimedMultiplierSpeedMod(duration, multiplier));

    public void TriggerTimedAdditive(float duration, float additive)
        => AddModifier(new TimedAdditiveSpeedMod(duration, additive));

    private void Update()
    {
        if (!autoTrigger) return;

        float now = Time.time;
        float dt = Time.deltaTime;

        // roll each configured auto buff
        for (int i = 0; i < autoBuffs.Count; i++)
        {
            var b = autoBuffs[i];
            if (!b.enabled) continue;
            if (now < b.nextAllowedTime) continue;

            if (Random.value < b.chancePerSecond * dt)
            {
                // spawn appropriate runtime modifier
                if (b.mode == SpeedBuffMode.Multiplier)
                    AddModifier(new TimedMultiplierSpeedMod(b.duration, b.value));
                else
                    AddModifier(new TimedAdditiveSpeedMod(b.duration, b.value));

                if (b.cooldown > 0f)
                    b.nextAllowedTime = now + b.cooldown;
            }
        }
    }

    /// Apply all active modifiers to baseSpeed for this frame.
    public float ApplyModifiers(float baseSpeed, float dt)
    {
        float totalMultiplier = 1f;
        float totalAdd = 0f;

        for (int i = mods.Count - 1; i >= 0; i--)
        {
            var m = mods[i];
            if (!m.Tick(dt))
            {
                mods.RemoveAt(i);
                continue;
            }

            // Combine by type
            if (m is TimedMultiplierSpeedMod mult)
                totalMultiplier *= Mathf.Max(0f, mult.Multiplier);
            else if (m is TimedAdditiveSpeedMod add)
                totalAdd += add.Additive;
        }

        return baseSpeed * totalMultiplier + totalAdd;
    }
}

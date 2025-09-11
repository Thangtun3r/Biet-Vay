using UnityEngine;
using System;
using System.Linq;
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

// Small container to track active mods with a tag so we can prevent stacking
internal class ActiveMod
{
    public ISpeedModifier mod;
    public string tag;              // e.g., AutoSpeedBuff.name
    public SpeedBuffMode mode;      // for ApplyModifiers policy
}

public class SpeedAffectoManager : MonoBehaviour
{
    [Header("Auto-Trigger Buffs")]
    [SerializeField] private bool autoTrigger = true;
    [SerializeField] private List<AutoSpeedBuff> autoBuffs = new();

    // Active runtime modifiers WITH TAGS (no stacking per tag)
    private readonly List<ActiveMod> mods = new();

    // --- Public API (manual add) ---
    // You can call these with a custom tag to prevent stacking that tag.
    public void AddModifier(ISpeedModifier mod, string tag, SpeedBuffMode mode)
    {
        // NO STACKING per tag: if a mod with the same tag is still alive, ignore this add
        if (!string.IsNullOrEmpty(tag) && mods.Any(m => m.tag == tag))
            return;

        mods.Add(new ActiveMod { mod = mod, tag = tag, mode = mode });
    }

    // Convenience helpers for manual triggers
    public void TriggerTimedMultiplier(float duration, float multiplier, string tag = "ManualMultiplier")
        => AddModifier(new TimedMultiplierSpeedMod(duration, multiplier), tag, SpeedBuffMode.Multiplier);

    public void TriggerTimedAdditive(float duration, float additive, string tag = "ManualAdditive")
        => AddModifier(new TimedAdditiveSpeedMod(duration, additive), tag, SpeedBuffMode.Additive);

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

            // Respect cooldown AND prevent re-trigger while an instance is active (no stacking per name)
            bool sameBuffActive = mods.Any(m => m.tag == b.name);
            if (sameBuffActive) continue;                 // already active → do not stack

            if (now < b.nextAllowedTime) continue;        // still on cooldown

            if (Random.value < b.chancePerSecond * dt)
            {
                // spawn appropriate runtime modifier (tag by buff name)
                if (b.mode == SpeedBuffMode.Multiplier)
                    AddModifier(new TimedMultiplierSpeedMod(b.duration, b.value), b.name, SpeedBuffMode.Multiplier);
                else
                    AddModifier(new TimedAdditiveSpeedMod(b.duration, b.value), b.name, SpeedBuffMode.Additive);

                // start cooldown timer (from now)
                if (b.cooldown > 0f)
                    b.nextAllowedTime = now + b.cooldown;
            }
        }
    }

    /// Apply active modifiers to baseSpeed for this frame.
    /// No stacking policy:
    ///   - Only the strongest multiplier is applied (max value).
    ///   - Only the additive with the largest absolute magnitude is applied.
    public float ApplyModifiers(float baseSpeed, float dt)
    {
        float bestMultiplier = 1f;
        float bestAdditive   = 0f;
        float bestAddAbs     = 0f;

        // Tick & prune
        for (int i = mods.Count - 1; i >= 0; i--)
        {
            var entry = mods[i];
            if (!entry.mod.Tick(dt))
            {
                // when this instance ends, it frees up its tag; next trigger after cooldown can happen
                mods.RemoveAt(i);
                continue;
            }

            if (entry.mode == SpeedBuffMode.Multiplier && entry.mod is TimedMultiplierSpeedMod mult)
            {
                // choose the strongest multiplier (not product)
                bestMultiplier = Mathf.Max(bestMultiplier, Mathf.Max(0f, mult.Multiplier));
            }
            else if (entry.mode == SpeedBuffMode.Additive && entry.mod is TimedAdditiveSpeedMod add)
            {
                // choose the additive with the largest absolute impact (positive or negative)
                float abs = Mathf.Abs(add.Additive);
                if (abs > bestAddAbs)
                {
                    bestAddAbs   = abs;
                    bestAdditive = add.Additive;
                }
            }
        }

        return baseSpeed * bestMultiplier + bestAdditive;
    }
}

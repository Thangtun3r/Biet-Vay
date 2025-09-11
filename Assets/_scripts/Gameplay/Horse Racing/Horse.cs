using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Horse2D : MonoBehaviour
{
    // ── Nested type to avoid Unity “ExtensionOfNativeClass” issues ──
    [Serializable]
    public struct AnimStage
    {
        [Tooltip("If currentSpeed >= this value, use 'multiplier'.")]
        public float threshold;

        [Tooltip("Animator 'Speed' parameter value to apply at/after this threshold.")]
        public float multiplier;
    }

    [Header("Base Stats")]
    public float speed = 8f;
    public float stamina = 100f;
    public float staminaDrain = 5f;
    public float minSpeedFactor = 0.3f;
    public float speedVariance = 0.5f;

    [Header("Tuning")]
    [Tooltip("Global scaler to make races faster/slower (e.g., 1.5 = 50% faster).")]
    [Min(0f)] public float speedMultiplier = 1f;

    public SpeedAffectoManager speedManager;
    public float currentSpeed;

    [Header("Smoothing")]
    [Tooltip("How quickly speed moves toward the target (units: 1/seconds). Higher = snappier.")]
    public float speedLerpPerSecond = 8f;

    // ── Animation control via stages ────────────────────────────────────────────
    [Header("Animation")]
    public Animator animator;

    [Tooltip("Animator float parameter name that the RunningCycle state uses as 'Multiplier'.")]
    public string speedParamName = "Speed";

    [Tooltip("List of thresholds and the multiplier to apply when reached.")]
    public AnimStage[] animStages =
    {
        new AnimStage{ threshold = 0f,  multiplier = 0.25f },
        new AnimStage{ threshold = 3f,  multiplier = 0.6f  },
        new AnimStage{ threshold = 6f,  multiplier = 1.0f  },
        new AnimStage{ threshold = 9f,  multiplier = 1.35f },
        new AnimStage{ threshold = 12f, multiplier = 1.7f  },
    };

    [Tooltip("If true, smoothly damp the parameter when it changes between stages.")]
    public bool smoothAnimParam = true;

    [Tooltip("How fast the animation parameter moves when smoothing is enabled.")]
    public float animParamLerpPerSecond = 10f;

    // cached id + runtime
    private int _speedParamID;
    private float _animParamCurrent = 1f;

    // ── Telemetry (optional) ───────────────────────────────────────────────────
    [Header("Telemetry")]
    [Range(0f, 1f)]
    [Tooltip("Race progress from 0 (start) to 1 (finish). Updated by RaceManager each frame.")]
    public float progress01 = 0f;

    public event Action<float> ProgressChanged;

    void Awake()
    {
        _speedParamID = Animator.StringToHash(speedParamName);
    }

    void Start()
    {
        currentSpeed = speed;
        if (animator != null)
        {
            _animParamCurrent = EvaluateAnimMultiplier(currentSpeed);
            animator.SetFloat(_speedParamID, _animParamCurrent);
        }
    }

    void Update()
    {
        // 0) Stamina drain
        stamina = Mathf.Max(0f, stamina - staminaDrain * Time.deltaTime);

        float stamina01     = Mathf.Clamp01(stamina / 100f);
        float staminaFactor = Mathf.Lerp(minSpeedFactor, 1f, stamina01);

        // 1) Base
        float baseStat = speed;

        // 2) External modifiers
        float modsApplied = (speedManager != null)
            ? speedManager.ApplyModifiers(baseStat, Time.deltaTime)
            : baseStat;

        // 3) Stamina AFTER modifiers
        float staminaReduced = modsApplied * staminaFactor;

        // 4) Variance from stamina
        float varianceNow  = Mathf.Lerp(0f, speedVariance, stamina01);
        float preMulTarget = Mathf.Max(0f, Random.Range(staminaReduced - varianceNow, staminaReduced));

        // 5) GLOBAL override
        float finalTarget = Mathf.Max(0f, preMulTarget * Mathf.Max(0f, speedMultiplier));

        // 6) Smooth into currentSpeed
        float maxDelta = speedLerpPerSecond * Time.deltaTime;
        currentSpeed = Mathf.MoveTowards(currentSpeed, finalTarget, maxDelta);

        // 7) Move
        transform.Translate(Vector3.right * currentSpeed * Time.deltaTime);

        // 8) Animation parameter via stages
        if (animator != null)
        {
            float targetAnim = EvaluateAnimMultiplier(currentSpeed);

            if (smoothAnimParam)
            {
                float aDelta = animParamLerpPerSecond * Time.deltaTime;
                _animParamCurrent = Mathf.MoveTowards(_animParamCurrent, targetAnim, aDelta);
            }
            else
            {
                _animParamCurrent = targetAnim; // step/jump behavior
            }

            animator.SetFloat(_speedParamID, _animParamCurrent);
        }
    }

    /// <summary>
    /// Look up the highest stage whose threshold is <= speed, and return its multiplier.
    /// If the list is empty, fall back to 1.
    /// </summary>
    private float EvaluateAnimMultiplier(float speedNow)
    {
        if (animStages == null || animStages.Length == 0)
            return 1f;

        float best = animStages[0].multiplier;
        float bestThreshold = float.NegativeInfinity;

        for (int i = 0; i < animStages.Length; i++)
        {
            var s = animStages[i];
            if (s.threshold <= speedNow && s.threshold >= bestThreshold)
            {
                bestThreshold = s.threshold;
                best = s.multiplier;
            }
        }
        return best;
    }

    /// <summary>Set by RaceManager. Clamped to [0,1].</summary>
    public void UpdateProgress(float p)
    {
        progress01 = Mathf.Clamp01(p);
        ProgressChanged?.Invoke(progress01);
    }
}

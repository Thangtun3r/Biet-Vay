using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Horse2D : MonoBehaviour
{
    [Serializable]
    public struct AnimStage
    {
        public float threshold;
        public float multiplier;
    }
    
   public int horseIndex = -1; // set by HorseRosterAssigner

    [Header("Base Stats")]
    public float speed = 8f;
    public float stamina = 100f;
    public float staminaDrain = 5f;
    public float minSpeedFactor = 0.3f;
    public float speedVariance = 0.5f;

    [Header("Tuning")]
    [Min(0f)] public float speedMultiplier = 1f;

    public SpeedAffectoManager speedManager;
    public float currentSpeed;

    [Header("Movement Target")]
    [Tooltip("Transform that actually moves forward. If null, will use this object's transform.")]
    public Transform moveTarget;

    [Header("Smoothing")]
    public float speedLerpPerSecond = 8f;

    [Header("Animation")]
    public Animator animator;
    public string speedParamName = "Speed";
    public AnimStage[] animStages =
    {
        new AnimStage{ threshold = 0f,  multiplier = 0.25f },
        new AnimStage{ threshold = 3f,  multiplier = 0.6f  },
        new AnimStage{ threshold = 6f,  multiplier = 1.0f  },
        new AnimStage{ threshold = 9f,  multiplier = 1.35f },
        new AnimStage{ threshold = 12f, multiplier = 1.7f  },
    };
    public bool smoothAnimParam = true;
    public float animParamLerpPerSecond = 10f;

    private int _speedParamID;
    private float _animParamCurrent = 1f;

    [Header("Telemetry")]
    [Range(0f, 1f)]
    public float progress01 = 0f;
    public event Action<float> ProgressChanged;

    void Awake()
    {
        _speedParamID = Animator.StringToHash(speedParamName);

        // If not assigned in inspector, default to self
        if (moveTarget == null)
            moveTarget = transform;
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

        float baseStat = speed;

        float modsApplied = (speedManager != null)
            ? speedManager.ApplyModifiers(baseStat, Time.deltaTime)
            : baseStat;

        float staminaReduced = modsApplied * staminaFactor;

        float varianceNow  = Mathf.Lerp(0f, speedVariance, stamina01);
        float preMulTarget = Mathf.Max(0f, Random.Range(staminaReduced - varianceNow, staminaReduced));

        float finalTarget = Mathf.Max(0f, preMulTarget * Mathf.Max(0f, speedMultiplier));

        float maxDelta = speedLerpPerSecond * Time.deltaTime;
        currentSpeed = Mathf.MoveTowards(currentSpeed, finalTarget, maxDelta);

        // ✅ Move chosen target instead of always self
        if (moveTarget != null)
            moveTarget.Translate(Vector3.right * currentSpeed * Time.deltaTime, Space.Self);

        // (Optional) Still support animator if assigned
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
                _animParamCurrent = targetAnim;
            }
            animator.SetFloat(_speedParamID, _animParamCurrent);
        }
    }

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

    public void UpdateProgress(float p)
    {
        progress01 = Mathf.Clamp01(p);
        ProgressChanged?.Invoke(progress01);
    }
}

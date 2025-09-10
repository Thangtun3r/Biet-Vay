using UnityEngine;

public class Horse2D : MonoBehaviour
{
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

    // --- ADDED ---
    [Header("Telemetry")]
    [Range(0f, 1f)]
    [Tooltip("Race progress from 0 (start) to 1 (finish). Updated by RaceManager each frame.")]
    public float progress01 = 0f;

    void Start()
    {
        currentSpeed = speed; // start near base so we don't lerp up from 0
    }

    void Update()
    {
        // Drain stamina
        stamina = Mathf.Max(0f, stamina - staminaDrain * Time.deltaTime);

        float stamina01     = Mathf.Clamp01(stamina / 100f);
        float staminaFactor = Mathf.Lerp(minSpeedFactor, 1f, stamina01);

        // 1) Apply base + multiplier
        float baseStat = speed * speedMultiplier;

        // 2) External modifiers
        float modsApplied = (speedManager != null)
            ? speedManager.ApplyModifiers(baseStat, Time.deltaTime)
            : baseStat;

        // 3) Apply stamina LAST
        float staminaReduced = modsApplied * staminaFactor;

        // 4) Variance
        float varianceNow = Mathf.Lerp(0f, speedVariance, stamina01);
        float targetSpeed = Mathf.Max(0f, Random.Range(staminaReduced - varianceNow, staminaReduced));

        // 5) Clamp changes instead of immediate lerp
        float maxDelta = speedLerpPerSecond * Time.deltaTime;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, maxDelta);

        // 6) Move
        transform.Translate(Vector3.right * currentSpeed * Time.deltaTime);
    }

    // --- ADDED ---
    /// <summary>Set by RaceManager. Clamped to [0,1].</summary>
    public void UpdateProgress(float p)
    {
        progress01 = Mathf.Clamp01(p);
    }
}

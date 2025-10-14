using UnityEngine;

public class TimeScaleHotkeys : MonoBehaviour
{
    [Header("Time Scales")]
    public float timeScale1 = 0.5f;
    public float timeScale2 = 1f;
    public float timeScale3 = 2f;
    public float timeScale4 = 4f;

    [Header("Debug Settings")]
    [Tooltip("Enable hotkeys (1–4) to change time scale. Automatically enabled in Editor, disabled in builds.")]
    [SerializeField]
    private bool enableHotkeys =
#if UNITY_EDITOR
        true;   // Enabled while testing in Editor
#else
        false;  // Disabled in builds
#endif

    private void Update()
    {
        if (!enableHotkeys) return; // Skip if disabled

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetTimeScale(timeScale1);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SetTimeScale(timeScale2);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            SetTimeScale(timeScale3);
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            SetTimeScale(timeScale4);
    }

    private void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // Keep physics consistent
        Debug.Log($"⏱ Time scale set to {scale}");
    }
}
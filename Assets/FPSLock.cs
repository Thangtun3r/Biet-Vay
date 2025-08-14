using UnityEngine;

public class FPSLock : MonoBehaviour
{
    [Header("Target Frames Per Second")]
    [Tooltip("Set your desired FPS here")]
    public int targetFPS = 60;

    void Start()
    {
        // Prevent VSync from overriding the target frame rate
        QualitySettings.vSyncCount = 0;

        // Apply the target FPS
        Application.targetFrameRate = targetFPS;
    }

    void Update()
    {
        // Allow runtime changes from the Inspector
        if (Application.targetFrameRate != targetFPS)
        {
            Application.targetFrameRate = targetFPS;
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class CreditRoll : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator creditAnimator;

    [Header("Animation Settings")]
    [Tooltip("The trigger name to start the credit roll animation.")]
    [SerializeField] private string startTriggerName = "Start";

    [Header("Scene Loading")]
    [Tooltip("Optional: Scene name to load after credits.")]
    [SerializeField] private string sceneToLoad;

    // --- YARN COMMAND INTEGRATION ---
    [YarnCommand("PlayCreditRoll")]
    public void Yarn_PlayCreditRoll()
    {
        PlayCreditRoll();
    }

    /// <summary>
    /// Public method to start the credit roll animation.
    /// </summary>
    public void PlayCreditRoll()
    {
        if (creditAnimator == null)
        {
            Debug.LogWarning("[CreditRoll] Animator not assigned.");
            return;
        }

        // Enable animator if it was disabled
        if (!creditAnimator.enabled)
            creditAnimator.enabled = true;

        // Trigger the animation
        if (!string.IsNullOrEmpty(startTriggerName))
            creditAnimator.SetTrigger(startTriggerName);

        Debug.Log("[CreditRoll] Credit roll animation started.");
    }

    /// <summary>
    /// Public method to load a specific scene by name.
    /// </summary>
    /// <param name="sceneName">Scene name to load. If empty, will use the serialized one.</param>
    public void LoadScene(string sceneName = "")
    {
        string targetScene = string.IsNullOrEmpty(sceneName) ? sceneToLoad : sceneName;

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning("[CreditRoll] No scene name provided to load.");
            return;
        }

        Debug.Log($"[CreditRoll] Loading scene: {targetScene}");
        SceneManager.LoadScene(targetScene);
    }
}
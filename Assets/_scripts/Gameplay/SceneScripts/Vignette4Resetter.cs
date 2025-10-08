using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

/// <summary>
/// Put this on a GameObject in any scene.
/// It will reset the Vignette4Looper counter to 0
/// only when the current scene name matches the configured reset scene name.
/// </summary>
public class Vignette4SceneResetter : MonoBehaviour
{
    [Header("Scene Matching")]
    [Tooltip("The exact name of the scene that triggers a reset.")]
    [SerializeField] private string resetSceneName = "ResetScene";

    [Header("Reset Options")]
    [Tooltip("Also mirror the reset into Yarn's $loopCount if a DialogueRunner is present.")]
    [SerializeField] private bool syncYarnVariable = true;

    [Tooltip("How long to keep looking for the looper this scene entry (seconds).")]
    [SerializeField] private float findTimeout = 3f;

    [Tooltip("Polling interval when waiting for the looper to appear (seconds).")]
    [SerializeField] private float pollInterval = 0.2f;

    private const string PlayerPrefsKey = "VignetteCounter";

    private void Start()
    {
        // Check if the current scene is the reset scene
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == resetSceneName)
        {
            // Only trigger the coroutine when in the matching scene
            StartCoroutine(ResetLooperThisSceneEntry());
        }
        else
        {
            Debug.Log($"[Vignette4SceneResetter] Scene '{currentScene}' does not match '{resetSceneName}', no reset triggered.");
        }
    }

    private IEnumerator ResetLooperThisSceneEntry()
    {
        float deadline = Time.unscaledTime + findTimeout;
        Vignette4Looper looper = null;

        while (Time.unscaledTime < deadline)
        {
            looper = FindObjectOfType<Vignette4Looper>();
            if (looper != null) break;
            yield return new WaitForSecondsRealtime(pollInterval);
        }

        if (looper == null)
        {
            Debug.LogWarning("[Vignette4SceneResetter] No Vignette4Looper found during this scene entry.");
            yield break;
        }

        // Reset counter and persist it
        looper.vignetteCounter = 0;
        PlayerPrefs.SetInt(PlayerPrefsKey, 0);
        PlayerPrefs.Save();

        if (syncYarnVariable)
        {
            var runner = looper.dialogueRunner != null ? looper.dialogueRunner : FindObjectOfType<DialogueRunner>();
            if (runner != null)
                runner.VariableStorage.SetValue("$loopCount", 0);
        }

        Debug.Log($"[Vignette4SceneResetter] Reset Vignette4Looper counter to 0 (Scene: {SceneManager.GetActiveScene().name}).");
    }
}

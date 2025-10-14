using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class Vignette4Looper : MonoBehaviour
{
    [Header("Yarn Integration")]
    public DialogueRunner dialogueRunner;

    [Header("Counter")]
    public int vignetteCounter = 0;

    [TextArea]
    public string resetNote =
        "Press number keys (1–8 or 0) to set the vignette counter directly.\nPress 9 to reset the counter.";

    private const string PlayerPrefsKey = "VignetteCounter";
    private static Vignette4Looper _instance;

    [Header("Auto Reset Settings")]
    [Tooltip("If the current scene name matches this, the looper counter will reset to 0 automatically.")]
    [SerializeField] private string resetSceneName = "Start Menu";

    [Tooltip("How long to wait for DialogueRunner to appear after scene load (seconds).")]
    [SerializeField] private float findTimeout = 3f;

    [Header("Debug Settings")]
    [Tooltip("Allow changing vignette counter using number keys (0–9) in the Editor. Automatically disabled in builds.")]
    [SerializeField] private bool enableDebugKeyInput =
    #if UNITY_EDITOR
        true;   // enabled in Editor
    #else
        false;  // disabled in builds
    #endif

    private void Awake()
    {
        // Ensure a single persistent instance
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        vignetteCounter = PlayerPrefs.GetInt(PlayerPrefsKey, 0);
        TryRebindDialogueRunner(logIfMissing: false);
    }

    private void OnEnable()
    {
        GameManager.OnResetScene += IncrementVignetteCounter;
        GameManager.OnResetScene += TryRebindDialogueRunner;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        GameManager.OnResetScene -= IncrementVignetteCounter;
        GameManager.OnResetScene -= TryRebindDialogueRunner;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnApplicationQuit()
    {
        SaveCounter();
    }

    private void Update()
    {
        if (dialogueRunner == null && Time.frameCount % 60 == 0)
        {
            TryRebindDialogueRunner(logIfMissing: false);
        }

        if (dialogueRunner != null)
        {
            dialogueRunner.VariableStorage.SetValue("$loopCount", vignetteCounter);
        }

        // Skip debug input if disabled
        if (!enableDebugKeyInput) return;

        for (int i = 0; i <= 9; i++)
        {
            KeyCode key = KeyCode.Alpha0 + i;
            if (Input.GetKeyDown(key))
            {
                HandleNumberInput(i);
                break;
            }
        }
    }

    private void HandleNumberInput(int number)
    {
        if (number == 9)
        {
            ResetVignetteCounter();
        }
        else
        {
            vignetteCounter = number;
            SaveCounter();
            Debug.Log($"[Vignette4Looper] Vignette counter set to {vignetteCounter}");
        }
    }

    public void IncrementVignetteCounter()
    {
        vignetteCounter++;
        SaveCounter();
    }

    private void ResetVignetteCounter()
    {
        vignetteCounter = 0;
        SaveCounter();
        Debug.Log("[Vignette4Looper] Vignette counter has been reset.");
    }

    private void SaveCounter()
    {
        PlayerPrefs.SetInt(PlayerPrefsKey, vignetteCounter);
        PlayerPrefs.Save();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryRebindDialogueRunner(logIfMissing: false);

        if (scene.name == resetSceneName)
        {
            Debug.Log($"[Vignette4Looper] Scene '{scene.name}' matches resetSceneName '{resetSceneName}'. Resetting counter.");
            ResetVignetteCounter();
        }
    }

    private void TryRebindDialogueRunner()
    {
        TryRebindDialogueRunner(logIfMissing: true);
    }

    private void TryRebindDialogueRunner(bool logIfMissing)
    {
        if (dialogueRunner != null) return;

        dialogueRunner = FindObjectOfType<DialogueRunner>();

        if (dialogueRunner != null)
        {
            Debug.Log("[Vignette4Looper] Rebound DialogueRunner.");
            dialogueRunner.VariableStorage.SetValue("$loopCount", vignetteCounter);
        }
        else if (logIfMissing)
        {
            Debug.LogWarning("[Vignette4Looper] No DialogueRunner found after scene change.");
        }
    }
}

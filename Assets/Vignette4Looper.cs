using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class Vignette4Looper : MonoBehaviour
{
    public DialogueRunner dialogueRunner;
    public int vignetteCounter = 0;

    [TextArea]
    public string resetNote =
        "Press number keys (1–8 or 0) to set the vignette counter directly.\nPress 9 to reset the counter.";

    private const string PlayerPrefsKey = "VignetteCounter";
    private static Vignette4Looper _instance;

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

        // Load saved vignette counter
        vignetteCounter = PlayerPrefs.GetInt(PlayerPrefsKey, 0);

        // Try to bind DialogueRunner immediately
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
        // If the DialogueRunner reference was destroyed, rebind occasionally
        if (dialogueRunner == null && Time.frameCount % 60 == 0)
        {
            TryRebindDialogueRunner(logIfMissing: false);
        }

        // Sync Yarn variable if available
        if (dialogueRunner != null)
        {
            dialogueRunner.VariableStorage.SetValue("$loopCount", vignetteCounter);
        }

        // Handle numeric key input (0–9)
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
            vignetteCounter = number; // 0 sets to 0; 1–8 set to their number
            SaveCounter();
            Debug.Log($"Vignette counter set to {vignetteCounter}");
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
        Debug.Log("Vignette counter has been reset.");
    }

    private void SaveCounter()
    {
        PlayerPrefs.SetInt(PlayerPrefsKey, vignetteCounter);
        PlayerPrefs.Save();
    }

    // --- Rebinding logic ---

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryRebindDialogueRunner(logIfMissing: false);
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
            Debug.Log("Vignette4Looper: Rebound DialogueRunner.");
            dialogueRunner.VariableStorage.SetValue("$loopCount", vignetteCounter);
        }
        else if (logIfMissing)
        {
            Debug.LogWarning("Vignette4Looper: No DialogueRunner found after scene change.");
        }
    }
}

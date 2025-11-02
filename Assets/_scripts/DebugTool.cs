using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;

// FMOD
using FMODUnity;
using FMOD.Studio;

public class DebugTool : MonoBehaviour
{
    [Header("Yarn (optional)")]
    [SerializeField] private DialogueRunner dialogueRunner; // assign if you use Yarn

    [Header("Scene / Menu")]
    [SerializeField] private string startMenuSceneName = "Start Menu";
    [SerializeField] private GameObject pauseMenuRoot; // the pause menu panel GameObject

    [Header("Buttons")]
    [SerializeField] private Button resetButton;       // restarts current scene
    [SerializeField] private Button backToStartButton; // loads start menu

    [Header("Cursor while paused")]
    [SerializeField] private bool enforceCursorEveryFrameWhilePaused = true;

    [Header("FMOD (optional)")]
    [Tooltip("If true, will pause FMOD buses / emitters on game pause and resume them on unpause.")]
    [SerializeField] private bool pauseFmodOnPause = true;

    [Tooltip("Bus paths to pause when paused. Leave as 'bus:/' to pause the Master bus. Add more buses if needed.")]
    [SerializeField] private string[] fmodBusPathsToPause = new[] { "bus:/" };

    [Tooltip("Specific EventEmitters to pause/resume alongside buses (useful for one-off or UI SFX).")]
    [SerializeField] private StudioEventEmitter[] fmodEmittersToPause;

    private bool isPaused = false;

    // Cache FMOD bus handles
    private Bus[] cachedBuses;

    // Cache emitter instances (built from fmodEmittersToPause)
    private EventInstance[] cachedEmitterInstances;

    // Store previous cursor state to restore on resume
    private bool prevCursorVisible;
    private CursorLockMode prevCursorLockState;

    private void Awake()
    {
        // Prepare FMOD handles early (safe even if lists are empty)
        BuildFmodCaches();
    }

    private void OnEnable()
    {
        GameManager.OnResetScene += ResetScene;

        if (resetButton)       resetButton.onClick.AddListener(ResetScene);
        if (backToStartButton) backToStartButton.onClick.AddListener(BackToStartMenu);

        // Ensure pause menu starts hidden and timescale/cursor normal
        SetPaused(false, force: true);
    }

    private void OnDisable()
    {
        GameManager.OnResetScene -= ResetScene;

        if (resetButton)       resetButton.onClick.RemoveListener(ResetScene);
        if (backToStartButton) backToStartButton.onClick.RemoveListener(BackToStartMenu);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (isPaused && enforceCursorEveryFrameWhilePaused)
        {
            ApplyPausedCursorState();
        }
    }

    public void TogglePause() => SetPaused(!isPaused);

    private void SetPaused(bool paused, bool force = false)
    {
        if (!force && paused == isPaused) return;

        isPaused = paused;

        // Show/hide pause menu UI
        if (pauseMenuRoot) pauseMenuRoot.SetActive(isPaused);

        // Time scale control
        Time.timeScale = isPaused ? 0f : 1f;

        // Cursor handling
        if (isPaused)
        {
            prevCursorVisible   = Cursor.visible;
            prevCursorLockState = Cursor.lockState;
            ApplyPausedCursorState();
        }
        else
        {
            RestoreCursorState();
        }

        // FMOD handling
        if (pauseFmodOnPause)
        {
            SetFmodPaused(isPaused);
        }
    }

    private void ApplyPausedCursorState()
    {
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void RestoreCursorState()
    {
        Cursor.visible   = prevCursorVisible;
        Cursor.lockState = prevCursorLockState;
    }

    private void ResetScene()
    {
        SetPaused(false, force: true);

        if (dialogueRunner && dialogueRunner.IsDialogueRunning)
            dialogueRunner.Stop();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    private void BackToStartMenu()
    {
        SetPaused(false, force: true);

        if (dialogueRunner && dialogueRunner.IsDialogueRunning)
            dialogueRunner.Stop();

        SceneManager.LoadScene(startMenuSceneName);
        Debug.Log($"[DebugTool] Returning to '{startMenuSceneName}'.");
    }

    // ---------- FMOD helpers ----------

    private void BuildFmodCaches()
    {
        // Build bus cache
        if (fmodBusPathsToPause == null || fmodBusPathsToPause.Length == 0)
        {
            cachedBuses = Array.Empty<Bus>();
        }
        else
        {
            cachedBuses = new Bus[fmodBusPathsToPause.Length];
            for (int i = 0; i < fmodBusPathsToPause.Length; i++)
            {
                try
                {
                    cachedBuses[i] = RuntimeManager.GetBus(fmodBusPathsToPause[i]);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[DebugTool] FMOD GetBus failed for '{fmodBusPathsToPause[i]}': {e.Message}");
                }
            }
        }

        // Build emitter instance cache
        if (fmodEmittersToPause == null || fmodEmittersToPause.Length == 0)
        {
            cachedEmitterInstances = Array.Empty<EventInstance>();
        }
        else
        {
            cachedEmitterInstances = new EventInstance[fmodEmittersToPause.Length];
            for (int i = 0; i < fmodEmittersToPause.Length; i++)
            {
                var emitter = fmodEmittersToPause[i];
                if (emitter == null) continue;

                try
                {
                    // Ensure the emitter has an instance (Start() would create one,
                    // but we avoid forcing playback—just grab/create safely).
                    // C#
                    var instance = emitter.EventInstance;
                    if (!instance.isValid())
                        instance = RuntimeManager.CreateInstance(emitter.EventReference);

                    cachedEmitterInstances[i] = instance;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[DebugTool] FMOD emitter cache failed for '{emitter.name}': {e.Message}");
                }
            }
        }
    }

    private void SetFmodPaused(bool paused)
    {
        // Pause/resume buses
        if (cachedBuses != null)
        {
            foreach (var bus in cachedBuses)
            {
                if (!bus.isValid()) continue;
                var result = bus.setPaused(paused);
                if (result != FMOD.RESULT.OK)
                    Debug.LogWarning($"[DebugTool] FMOD Bus.setPaused({paused}) => {result}");
            }
        }

        // Pause/resume specific emitters (if any)
        if (cachedEmitterInstances != null)
        {
            foreach (var inst in cachedEmitterInstances)
            {
                if (!inst.isValid()) continue;
                var result = inst.setPaused(paused);
                if (result != FMOD.RESULT.OK)
                    Debug.LogWarning($"[DebugTool] FMOD EventInstance.setPaused({paused}) => {result}");
            }
        }
    }
}

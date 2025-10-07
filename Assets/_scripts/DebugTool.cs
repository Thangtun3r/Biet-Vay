using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class DebugTool : MonoBehaviour
{
    [SerializeField] private DialogueRunner dialogueRunner; // assign in Inspector if you use Yarn
    [SerializeField] private string startMenuSceneName = "Start Menu";
    [SerializeField] private float holdDuration = 1f; // seconds to hold ESC before returning

    private float escHoldTimer = 0f;
    private bool isHoldingEsc = false;

    private void OnEnable()
    {
        GameManager.OnResetScene += ResetScene;
    }

    private void OnDisable()
    {
        GameManager.OnResetScene -= ResetScene;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetScene();
        }

        // ESC hold logic
        if (Input.GetKey(KeyCode.Escape))
        {
            if (!isHoldingEsc)
            {
                isHoldingEsc = true;
                escHoldTimer = 0f;
            }

            escHoldTimer += Time.unscaledDeltaTime;

            if (escHoldTimer >= holdDuration)
            {
                BackToStartMenu();
                isHoldingEsc = false;
                escHoldTimer = 0f;
            }
        }
        else if (Input.GetKeyUp(KeyCode.Escape))
        {
            // Reset if released early
            isHoldingEsc = false;
            escHoldTimer = 0f;
        }
    }

    private void ResetScene()
    {
        if (dialogueRunner && dialogueRunner.IsDialogueRunning)
        {
            dialogueRunner.Stop();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    private void BackToStartMenu()
    {
        Time.timeScale = 1f;

        if (dialogueRunner && dialogueRunner.IsDialogueRunning)
            dialogueRunner.Stop();

        SceneManager.LoadScene(startMenuSceneName);
        Debug.Log($"[DebugTool] Returning to '{startMenuSceneName}' after holding ESC for {holdDuration} seconds.");
    }
}

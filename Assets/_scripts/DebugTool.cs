using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class DebugTool : MonoBehaviour
{
    [SerializeField] private DialogueRunner dialogueRunner; // assign in Inspector if you use Yarn

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetScene();
        }
    }

    private void ResetScene()
    {
        // 1. Stop any running dialogue to avoid callbacks after reload
        if (dialogueRunner && dialogueRunner.IsDialogueRunning)
        {
            dialogueRunner.Stop();
        }

        // 2. Reload the active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // 3. Force cleanup of destroyed objects/resources
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }
}
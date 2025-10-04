using System;
using UnityEngine;
using Yarn.Unity;

public class Vignette4Looper : MonoBehaviour
{
    public DialogueRunner dialogueRunner;
    public int vignetteCounter = 0;

    [TextArea]
    public string resetNote = 
        "Press number keys (1–8 or 0) to set the vignette counter directly.\nPress 9 to reset the counter.";

    private void Awake()
    {
        if (PlayerPrefs.HasKey("VignetteCounter"))
        {
            vignetteCounter = PlayerPrefs.GetInt("VignetteCounter");
        }
        else
        {
            vignetteCounter = 0;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        GameManager.OnResetScene += IncrementVignetteCounter;
    }

    private void OnDisable()
    {
        GameManager.OnResetScene -= IncrementVignetteCounter;
    }

    private void OnApplicationQuit()
    {
        SaveCounter();
    }

    private void Update()
    {
        // Sync with Yarn variable
        dialogueRunner.VariableStorage.SetValue("$loopCount", vignetteCounter);

        // Check for numeric key input (1–9 and 0)
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
            // Special case: 0 sets the counter to 0, others set to their number
            vignetteCounter = number;
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
        PlayerPrefs.SetInt("VignetteCounter", vignetteCounter);
        PlayerPrefs.Save();
    }
}

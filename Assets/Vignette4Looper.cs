using System;
using UnityEngine;
using Yarn.Unity;

public class Vignette4Looper : MonoBehaviour
{
    public DialogueRunner dialogueRunner;
    // The public field to store the integer
    public int vignetteCounter = 0;

    // A note in the script for reference (visible in the Inspector)
    [TextArea]
    public string resetNote = "Press the 9 key to reset the vignette counter.";

    private void Awake()
    {
        // Ensure the data persists across scene resets
        if (PlayerPrefs.HasKey("VignetteCounter"))
        {
            // Load saved data if it exists
            vignetteCounter = PlayerPrefs.GetInt("VignetteCounter");
        }
        else
        {
            // Initialize it if no saved data is found
            vignetteCounter = 0;
        }

        // Ensure this object is not destroyed when the scene resets
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
        // Save the current value of vignetteCounter when the application closes
        PlayerPrefs.SetInt("VignetteCounter", vignetteCounter);
        PlayerPrefs.Save();
    }

    private void Update()
    {
        dialogueRunner.VariableStorage.SetValue("$loopCount", vignetteCounter);
        // Listen for the 9 key press to reset the vignette counter
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            ResetVignetteCounter();
        }
    }

    public void IncrementVignetteCounter()
    {
        // Increment the counter
        vignetteCounter++;
        
        
        // Save the updated counter value
        PlayerPrefs.SetInt("VignetteCounter", vignetteCounter);
        PlayerPrefs.Save();
    }

    // Reset the vignette counter to 0 and update PlayerPrefs
    private void ResetVignetteCounter()
    {
        vignetteCounter = 0;
        PlayerPrefs.SetInt("VignetteCounter", vignetteCounter);
        PlayerPrefs.Save();
        Debug.Log("Vignette counter has been reset.");
    }
}
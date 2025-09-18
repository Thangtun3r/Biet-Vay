using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class FMODYarnEvent : MonoBehaviour
{
    [Header("FMOD Settings")]
    [Tooltip("Prefix that will be added before the sound name.")]
    public string soundPrefix = "event:/SFX/";  // Default prefix

    private static FMODYarnEvent instance;

    private void Awake()
    {
        // Store reference so static method can access instance fields
        instance = this;
    }

    [YarnCommand("playSound")]
    public static void PlaySound(string soundName)
    {
        if (instance == null)
        {
            Debug.LogError("FMODYarnEvent instance not found in the scene.");
            return;
        }

        string fullPath = instance.soundPrefix + soundName;
        FMODUnity.RuntimeManager.PlayOneShot(fullPath);
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class YarnDialogueEventBridge : MonoBehaviour
{
    //this work as an communication bridge between Interactable Objects that contain node string and the script that handle calling Yarn Dialogue Runner.
    public static event Action<string> OnYarnEventCalled;

    public static void CallYarnEvent(string eventName)
    {
        OnYarnEventCalled?.Invoke(eventName);
    }
}


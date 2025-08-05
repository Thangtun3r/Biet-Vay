using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class YarnEventCaller : MonoBehaviour
{
    public string eventName;

    public static event Action<string> OnYarnEventCalled; 
    
    public void CallYarnEvent()
    {
        OnYarnEventCalled?.Invoke(eventName);
    }
}

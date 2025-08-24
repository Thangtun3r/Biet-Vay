using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switcher : MonoBehaviour
{
    public static event Action OnSwitch;
    private void Update()
    {
        HandleSwitch();
    }
    
    private void HandleSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OnSwitch?.Invoke();
        }
    }
}

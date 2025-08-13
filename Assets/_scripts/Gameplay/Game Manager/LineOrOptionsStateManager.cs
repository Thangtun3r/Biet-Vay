using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class LineOrOptionsStateManager : MonoBehaviour
{
    public GameObject[] stuffsToDisableWhenOptionsDisplayed;
    public void OnEnable()
    {
        WordDisplayManager.OnOptionsDisplayed += OnOptionsDisplayed;
        LinePresenter.OnLineDisplayed += OnLineDisplayed;
    }
    
    public void OnDisable()
    {
        WordDisplayManager.OnOptionsDisplayed -= OnOptionsDisplayed;
    }
    
    public void OnOptionsDisplayed()
    {
        foreach (var stuff in stuffsToDisableWhenOptionsDisplayed)
        {
            stuff.SetActive(true);
        }
    }
    
    public void OnLineDisplayed()
    {
        foreach (var stuff in stuffsToDisableWhenOptionsDisplayed)
        {
            stuff.SetActive(false);
        }
    }
    
    
}

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
        WordDisplayManager.OnOptionsDisplayed += OnEnableObjects;
        LinePresenter.OnLineDisplayed += OnDisableObjects;
        GameTransition.OnExpandStarted += OnDisableObjects; // Ensure line is displayed when expanding
    }
    
    public void OnDisable()
    {
        WordDisplayManager.OnOptionsDisplayed -= OnEnableObjects;
        LinePresenter.OnLineDisplayed -= OnDisableObjects;
    }
    
    public void OnEnableObjects()
    {
        foreach (var stuff in stuffsToDisableWhenOptionsDisplayed)
        {
            stuff.SetActive(true);
        }
    }
    
    public void OnDisableObjects()
    {
        foreach (var stuff in stuffsToDisableWhenOptionsDisplayed)
        {
            stuff.SetActive(false);
        }
    }
    

}

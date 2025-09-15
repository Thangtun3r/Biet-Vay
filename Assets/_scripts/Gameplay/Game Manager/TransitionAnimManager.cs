using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionAnimManager : MonoBehaviour
{
    [SerializeField] private Animator TransitionAnimator;


    private void OnEnable()
    {
        GameManager.OnTransition += PlayTransition;
    }
    
    private void OnDisable()
    {
        GameManager.OnTransition -= PlayTransition;
    }

    public void PlayTransition(int transitionID)
    {
    
        TransitionAnimator.SetInteger("AnimID", transitionID);
    }
    
    
}

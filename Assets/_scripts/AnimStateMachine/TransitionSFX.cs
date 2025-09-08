using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class TransitionSFX : StateMachineBehaviour
{
    public string FMODEvent;
    public string FMODBGMParamName;
    public float FMODParamValue;
    public static event Action<string,float> OnTransition;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        RuntimeManager.PlayOneShot(FMODEvent);
        OnTransition?.Invoke(FMODBGMParamName, FMODParamValue);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //animator.SetBool("Rect", false);
        //animator.SetBool("Fade", false);
    }
    
    
}
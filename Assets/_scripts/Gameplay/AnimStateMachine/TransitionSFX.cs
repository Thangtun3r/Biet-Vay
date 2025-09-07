using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class TransitionSFX : StateMachineBehaviour
{
    public string FMODEvent;
    public string FMODBGM;
    public static event Action<string> OnTransition;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       RuntimeManager.PlayOneShot(FMODEvent);
       OnTransition?.Invoke(FMODBGM);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Rect", false);
        animator.SetBool("Fade", false);
    }
}

using System;
using FMODUnity;
using UnityEngine;
using Yarn.Unity;

public class BettingSlip : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Animator that controls the Betting Slip animations.")]
    public Animator animator;

    [Tooltip("Trigger name in the Animator to activate on click.")]
    public string triggerName = "Flip";
    
    public GameObject bettingSlip;

    private void OnEnable()
    {
        FinishLineTrigger.OnWinnerDetermined += UncurveSlip;
    }
    
    private void OnDisable()
    {
        FinishLineTrigger.OnWinnerDetermined -= UncurveSlip;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TriggerAnimation();
        }
    }

    private void TriggerAnimation()
    {
        if (animator == null) return;
        
        animator.SetTrigger(triggerName);
    }
    
    [YarnCommand("enableSlip")]
    public void EnableSlip()
    {
        animator.enabled = true;
        bettingSlip.SetActive(true);
    }
    
    public void UncurveSlip(int winnerIndex)
    {
        animator.SetBool("UnCurve", true);
        RuntimeManager.PlayOneShot("event:/Vignette 2/Slip_Unfold");
    }

    [YarnCommand("RedeemSlip")]
    public void RedeemSlip()
    {
        animator.enabled = false;
        bettingSlip.SetActive(false);
    }
    
    public void PlayHitSound()
    {
        RuntimeManager.PlayOneShot("event:/Vignette 2/Slip Hit");
    }


}
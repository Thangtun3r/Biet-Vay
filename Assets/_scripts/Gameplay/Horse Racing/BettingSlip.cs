using System;
    using FMODUnity;
    using UnityEngine;
    using Yarn.Unity;
    
    public class BettingSlip : MonoBehaviour
    {
        [Header("Animation Settings")]
        [Tooltip("Animator that controls the Betting Slip animations.")]
        public Animator animator;
        public static event Action OnSlipEnabled;
    
        [Tooltip("Trigger name in the Animator to activate on click.")]
        public string triggerName = "Flip";
        
        public GameObject bettingSlip;
    
        // Tracks whether we've already skipped the first hit sound
        private bool hasSkippedFirstHitSound;
    
        private void OnEnable()
        {
            FinishLineTrigger.OnWinnerDetermined += UncurveSlip;
            AnotherBettingDay.OnRaceReset += CurveSlip;
        }
        
        private void OnDisable()
        {
            FinishLineTrigger.OnWinnerDetermined -= UncurveSlip;
            AnotherBettingDay.OnRaceReset -= CurveSlip;
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
            OnSlipEnabled?.Invoke();
            animator.enabled = true;
            bettingSlip.SetActive(true);
            // Reset so the first PlayHitSound call after enabling is ignored
            hasSkippedFirstHitSound = false;
        }
        
        public void UncurveSlip(int winnerIndex)
        {
            animator.SetBool("UnCurve", true);
            RuntimeManager.PlayOneShot("event:/Vignette 2/Slip_Unfold");
        }
        
        public void CurveSlip()
        {
            animator.SetBool("UnCurve", false);
        }
    
        [YarnCommand("RedeemSlip")]
        public void RedeemSlip()
        {
            animator.enabled = false;
            bettingSlip.SetActive(false);
        }
        
        public void PlayHitSound()
        {
            // Skip the first invocation
            if (!hasSkippedFirstHitSound)
            {
                hasSkippedFirstHitSound = true;
                return;
            }
    
            RuntimeManager.PlayOneShot("event:/Vignette 2/Slip Hit");
            RuntimeManager.PlayOneShot("event:/Vignette 2/Horse Cheer");
        }
    }
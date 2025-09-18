using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Yarn.Unity;

public class ATMMachineManager : MonoBehaviour
{
    [Header(("Yarn Dialogue Manager here"))]
    [SerializeField] private DialogueRunner dialogueRunner;
    
    [Header(("ATM Component here"))]
    public GameObject CardInsertedUI;
    public TextMeshProUGUI balanceText;
    public Animator ATMAnimator;

    private float currentBalance = 0f;
    private float depositAmount = 0f;
    private float withdrawAmount = 0f;
    
    
    private void OnEnable()
    {
        ATMMachineEvent.OnCardInsert += HideCardInsertedUI;
        ATMMachineEvent.OnDeposit += HandleDeposit;
        ATMMachineEvent.OnWithdraw += HandleWithdraw;
    }
    
    private void OnDisable()
    {
        ATMMachineEvent.OnCardInsert -= HideCardInsertedUI;
        ATMMachineEvent.OnDeposit -= HandleDeposit;
        ATMMachineEvent.OnWithdraw -= HandleWithdraw;
    }
    
    private void HideCardInsertedUI()
    {
        ATMAnimator.SetInteger("ActionID", 1);
    }
    
    
    private void HandleWithdraw()
    {
        ATMAnimator.SetInteger("ActionID", 2);
    }
    
    private void HandleDeposit()
    {
        ATMAnimator.SetInteger("ActionID", 3);
    }

    private void Update()
    {
        dialogueRunner.VariableStorage.TryGetValue("$currentBalance", out currentBalance);
        balanceText.text =currentBalance.ToString("N2") + "VND";
    }
}

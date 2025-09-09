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
        CardInsertedUI.SetActive(false);
        dialogueRunner.VariableStorage.TryGetValue("$CurrentBalance", out currentBalance);
        balanceText.text = "$" + currentBalance.ToString("F2");
    }
    
    
    private void HandleWithdraw()
    {
        dialogueRunner.VariableStorage.TryGetValue("$CurrentBalance", out currentBalance);
        balanceText.text = "$" + currentBalance.ToString("F2");
    }
    
    private void HandleDeposit()
    {
        dialogueRunner.VariableStorage.TryGetValue("$CurrentBalance", out currentBalance);
        balanceText.text = "$" + currentBalance.ToString("F2");
    }
}

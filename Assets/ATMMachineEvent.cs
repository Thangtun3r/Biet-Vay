using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class ATMMachineEvent : MonoBehaviour
{
    public static event Action OnCardInsert;
    public static event Action OnDeposit;
    public static event Action OnWithdraw;
    
    [YarnCommand ("cardInsert")]
    public static void CardInsert()
    {
      OnCardInsert?.Invoke();
    }
    
    [YarnCommand ("deposit")]
    public static void Deposit()
    {
        OnDeposit?.Invoke();
    }
    
    [YarnCommand ("withdraw")]
    public static void Withdraw()
    {
        OnWithdraw?.Invoke();
    }
    
}

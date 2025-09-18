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
    public static IEnumerator CardInsert()
    {
      OnCardInsert?.Invoke();
        yield return new WaitForSeconds(0.4f);
    }
    
    [YarnCommand ("deposit")]
    public static IEnumerator Deposit()
    {
        OnDeposit?.Invoke();
        yield return new WaitForSeconds(4f);
    }
    
    [YarnCommand ("withdraw")]
    public static IEnumerator Withdraw()
    {
        OnWithdraw?.Invoke();
        yield return new WaitForSeconds(4f);
    }
    
}

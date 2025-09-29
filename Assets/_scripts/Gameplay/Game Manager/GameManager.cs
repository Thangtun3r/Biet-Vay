using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Yarn.Unity;

public class GameManager : MonoBehaviour
{
    //This part is very hard coded, will change later but this is use to flag for the wordpool to clear the pool setting it's isResolved bool 
    public static event Action OnBietvay;
    
    // For horse Betting event
    public static event Action<int> OnBetPlaced;
    public static event Action OnRaceStart;
    public static event Action OnPayout;
    
    //For controlling the UI 
    public static event Action OnExpand;
    //Control player
    public static event Action OnFreeze;
    public static event Action OnUnFreeze;
    
    public static event Action disablePlayerMovement;
    public static event Action enablePlayerMovement;
    //Control scene transition
    public static event Action<int> OnTransition;
    public static event Action OnWeather;
    
    //Control Cinemachine
    public static event Action<int> OnVMBoost;
    public static event Action OnVMReset;
    
    //Control Scene Reset
    public static event Action OnResetScene;
    
    public static event Action<int> OnPropEnable;
    
    public static event Action OnResolveAnim;
    
    // deltete after testing
    public static event Action OnPsudoTurnOff;
    public static event Action OnPsudoTurnOn;
    
    //Event for gacha machine
    public static event Action OnRollGacha;


    [YarnCommand("disablePlayer")]
    public static void DisablePlayer()
    {
        disablePlayerMovement?.Invoke();
    }
    

    [YarnCommand("collapse")]
    public static IEnumerator Collapse()
    {
        GameTransition.Instance.Collapse();
        yield return new WaitForSeconds(0.6f);
    } 

    [YarnCommand("expand")]
    public static IEnumerator Expand()
    {
        OnExpand?.Invoke();
        GameTransition.Instance.Expand();
        yield return new WaitForSeconds(1f);
        enablePlayerMovement?.Invoke();
    } 

    [YarnCommand("spawn")]
    public static IEnumerator SpawnPoint(string spawnPointName)
    {
        var name = (spawnPointName ?? string.Empty).Trim();

        if (!SpawnPointHandler.TryGetSpawnPoint(name, out var point) || point == null)
        {
            Debug.LogError($"[GameManager] Yarn <<spawn {name}>>: spawn point not found.");
            yield break; // dialogue continues
        }

        SpawnPointHandler.InvokePlayerSpawn(point);

        yield return new WaitForFixedUpdate();
        yield return null;
        yield return new WaitForEndOfFrame();
    }

    [YarnCommand("freeze")]
    public static void Freeze() => OnFreeze?.Invoke();

    [YarnCommand("unfreeze")]
    public static void Unfreeze() => OnUnFreeze?.Invoke();

    //temporary weather command for testing purposes
    [YarnCommand("weather")]
    public static void Weather()
    {
        OnWeather?.Invoke();
    }


    [YarnCommand("VMBoost")]
    public static IEnumerator cinemachine(int cinemachineID)
    {
        disablePlayerMovement?.Invoke();
        OnVMBoost?.Invoke(cinemachineID);
        yield return new WaitForSeconds(1.1f);
    }

    [YarnCommand("VMReset")]
    public static  IEnumerator cinemachineReset()
    {
        disablePlayerMovement?.Invoke();
        OnVMReset?.Invoke();
        yield return new WaitForSeconds(1.1f);
        enablePlayerMovement?.Invoke();
    }
    
    
    //also delete after testing
    [YarnCommand("PsudoTurnOff")]
    public static void PsudoTurnOff()
    {
        OnPsudoTurnOff?.Invoke();
    }
    
    [YarnCommand("PsudoTurnOn")]
    public static void PsudoTurnOn()
    {
        OnPsudoTurnOn?.Invoke();
    }
    
    [YarnCommand("transition")]

    public static IEnumerator Transition(int transitionID)
    {
        disablePlayerMovement.Invoke();
        OnTransition?.Invoke(transitionID);
        yield return new WaitForSeconds(1f);
        enablePlayerMovement.Invoke();
    }
    
    [YarnCommand("prop")]
    public static void PropEnable(int propID)
    {
        OnPropEnable?.Invoke(propID);
    }
    
    [YarnCommand("restartScene")]
    public static void RestartScene()
    {
        OnResetScene?.Invoke();
    }

    [YarnCommand("resolve")]
    public static void Resolve()
    {
        OnResolveAnim?.Invoke();
    }
        
    [YarnCommand("isBietVay")]
    public static void IsBietvay()
    {
        OnBietvay?.Invoke();
    }
    
    /// <summary>
    /// Horse betting command
    /// </summary>
    /// <param name="horseIndex"></param>
    /// <param name="amount"></param>
    
    [YarnCommand("placeBet")]
    public static void PlacedBet(int horseIndex)
    {
        OnBetPlaced?.Invoke(horseIndex);
    }

    [YarnCommand("startRace")]
    public static void StartRace()
    {
        OnRaceStart?.Invoke();
    }
    
    [YarnCommand("payOut")]
    public static void PayOut()
    {
        OnPayout.Invoke();
    }
    
    
    // This part is to control the gacha machine
    [YarnCommand("roll")]
    public static IEnumerator HandleRoll()
    {
        // Let the VM exit the option-selection phase
        yield return null;

        bool done = false;
        void OnDone(GachaObjectSO _) { done = true; }

        GachaMachine.OnGachaRolled += OnDone;
        OnRollGacha?.Invoke();          // triggers animator.SetTrigger("Roll")

        // Wait until the result event fires (this is when $favoriteGashapon is set)
        while (!done) yield return null;

        GachaMachine.OnGachaRolled -= OnDone;
    }
    
    
    [YarnCommand("resetScene")]
    public static void ResetScene()
    {
        OnResetScene?.Invoke();
    }
    
    [YarnCommand("UnlockMouse")]
    public static void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

}
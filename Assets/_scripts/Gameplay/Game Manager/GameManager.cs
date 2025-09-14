using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Yarn.Unity;

public class GameManager : MonoBehaviour
{
    // For horse Betting event
    public static event Action<int> OnBetPlaced;
    public static event Action OnRaceStart;
    public static event Action OnPayout;
    
    //For controlling the UI 
    public static event Action OnExpand;
    public static event Action OnFreeze;
    public static event Action OnUnFreeze;
    public static event Action<int> OnTransition;
    public static event Action OnWeather;
    public static event Action<int> OnVMBoost;
    public static event Action OnVMReset;
    
    public static event Action OnResetScene;
    
    public static event Action<int> OnPropEnable;
    
    public static event Action OnResolveAnim;
    
    // deltete after testing
    public static event Action OnPsudoTurnOff;
    public static event Action OnPsudoTurnOn;


    [YarnCommand("collapse")]
    public static IEnumerator Collapse()
    {
        GameTransition.Instance.Collapse();
        yield return new WaitForSeconds(0.6f);
    } 

    [YarnCommand("expand")]
    public static void Expand()
    {
        OnExpand?.Invoke();
        GameTransition.Instance.Expand();
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
    public static void cinemachine(int cinemachineID)
    {
        OnVMBoost?.Invoke(cinemachineID);
    }

    [YarnCommand("VMReset")]
    public static void cinemachineReset()
    {
        OnVMReset?.Invoke();
    }
    
    
    //also delete after testing
    [YarnCommand("PsudoTurnOff")]
    public static void PsudoTurnOff()
    {
        OnPsudoTurnOff?.Invoke();
        Debug.Log("Psudo Turn Off Called");
    }
    
    [YarnCommand("PsudoTurnOn")]
    public static void PsudoTurnOn()
    {
        OnPsudoTurnOn?.Invoke();
        Debug.Log("Psudo Turn Off Called");
    }
    
    [YarnCommand("transition")]

    public static IEnumerator Transition(int transitionID)
    {
        OnTransition?.Invoke(transitionID);
        yield return new WaitForSeconds(1f);
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
}
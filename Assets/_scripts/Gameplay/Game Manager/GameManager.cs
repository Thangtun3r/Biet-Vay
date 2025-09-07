using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Yarn.Unity;

public class GameManager : MonoBehaviour
{
    
    public static event Action OnFreeze;
    public static event Action OnUnFreeze;
    public static event Action<int> OnTransition;
    public static event Action OnWeather;
    public static event Action<int> OnVMBoost;
    public static event Action OnVMReset;
    
    // deltete after testing
    public static event Action OnPsudoTurnOff;
    public static event Action OnPsudoTurnOn;
    
    
    [YarnCommand("collapse")]
    public static void Collapse() => GameTransition.Instance.Collapse();

    [YarnCommand("expand")]
    public static void Expand() => GameTransition.Instance.Expand();

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
    
}
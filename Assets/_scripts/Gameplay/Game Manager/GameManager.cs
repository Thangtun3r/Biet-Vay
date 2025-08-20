using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Yarn.Unity;

public class GameManager : MonoBehaviour
{
    
    public static event Action OnFreeze;
    public static event Action OnTransition;
    public static event Action OnWeather;
    
    
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
    public static void Freeze()
    {
        OnFreeze?.Invoke();
    }

    [YarnCommand("transition")]
    public static void Transition()
    {
        OnTransition?.Invoke();
    }

    //temporary weather command for testing purposes
    [YarnCommand("weather")]
    public static void Weather()
    {
        OnWeather?.Invoke();
    }

}
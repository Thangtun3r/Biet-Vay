using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

//This will be the centralized manager for the game handeling event to communnicate between Yarn and Unity.
public class GameManager : MonoBehaviour
{
    
    [YarnCommand("collapse")]
    public static void Collapse()
    {
        GameTransition.Instance.Collapse();
    }
    
    
    [YarnCommand("expand")]
    public static void Expand()
    {
        GameTransition.Instance.Expand();
    }

    [YarnCommand("spawn")]
    public static void SpawnPoint(string spawnPointName)
    {
        SpawnPointHandler.SetPlayerSpawnPoint(spawnPointName);
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RaceResetter : MonoBehaviour
{
    public static event Action OnRaceReset;
    private RaceManager m_RaceManager;

    private void Awake()
    {
        m_RaceManager = GetComponent<RaceManager>();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            HandleResetRace();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            HandleStartRace();
        }
    }
    
    private void HandleResetRace()
    {
        OnRaceReset?.Invoke();
    }
    private void HandleStartRace()
    {
        m_RaceManager.TriggerStart();
    }
}

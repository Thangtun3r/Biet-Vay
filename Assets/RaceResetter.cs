using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class RaceResetter : MonoBehaviour
{
    public static event Action OnRaceReset;
    public Image preRaceFill;
    private RaceManager raceManager;

    private void Awake()
    {
        raceManager = GetComponent<RaceManager>();
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
        preRaceFill.DOFade(1, 0f);
        OnRaceReset?.Invoke();
    }
    private void HandleStartRace()
    {
        raceManager.TriggerStart();
    }
}

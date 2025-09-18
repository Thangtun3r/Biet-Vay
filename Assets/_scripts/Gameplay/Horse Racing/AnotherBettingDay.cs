using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;


public class AnotherBettingDay : MonoBehaviour
{
    public static event Action OnRaceReset;
    public Image preRaceFill;
    public RaceManager raceManager;


    [YarnCommand("newBettingDay")]
    public static void NewBettingDay()
    {
        OnRaceReset?.Invoke();
    }

    private void Awake()
    {
        raceManager = GetComponent<RaceManager>();
    }
    
    private void HandleResetRace()
    {
        preRaceFill.DOFade(1, 0f);
        OnRaceReset?.Invoke();
    }
}

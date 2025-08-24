using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PsudoTurnOff : MonoBehaviour
{
    public GameObject psudoImagg;

    private void OnEnable()
    {
        GameManager.OnPsudoTurnOff += psudoTurnOff;
        GameManager.OnPsudoTurnOn += psudoTurnOn;
    }

    private void OnDisable()
    {
        GameManager.OnPsudoTurnOff -= psudoTurnOff;
        GameManager.OnPsudoTurnOn -= psudoTurnOn;
    }


    private void psudoTurnOff()
    {
        psudoImagg.SetActive(false);
    }
    
    private void psudoTurnOn()
    {
        psudoImagg.SetActive(true);
    }
}

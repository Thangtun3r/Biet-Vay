using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PsudoTurnOff : MonoBehaviour
{
    [Header("Assign multiple GameObjects here")]
    public GameObject[] psudoImaggArray; // <-- Now it's an array

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
        foreach (GameObject obj in psudoImaggArray)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    private void psudoTurnOn()
    {
        foreach (GameObject obj in psudoImaggArray)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }
}
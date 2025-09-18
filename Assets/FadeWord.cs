using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay;
using UnityEngine;

public class FadeWord : MonoBehaviour
{
    public bool isFadeWord;

    public Words _words;

    private void OnEnable()
    {
        _words.enabled = true;
    }

    private void Start()
    {

        _words = GetComponent<Words>();
    }


    private void Update()
    {
        if (isFadeWord)
        {
            _words.enabled = false;
        }
    }
}

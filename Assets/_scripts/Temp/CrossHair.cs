using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CrossHair : MonoBehaviour
{
    private Image _image;


    private void Start()
    {
        _image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        GameTransition.OnCollapseStarted += HideCrosshair;
        GameTransition.OnExpandStarted += ShowCrosshair;
    }
    
    private void OnDisable()
    {
        GameTransition.OnCollapseStarted -= HideCrosshair;
        GameTransition.OnExpandStarted -= ShowCrosshair;
    }
    
    private void HideCrosshair()
    {
        _image.DOFade(0f, 0f);
    }
    private void ShowCrosshair()
    {
        _image.DOFade(1f, 0.2f);
    }
}

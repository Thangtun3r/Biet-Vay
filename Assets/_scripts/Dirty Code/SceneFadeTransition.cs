using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SceneFadeTransition : MonoBehaviour
{
    private Image fadeImage;
    private float fadeDuration = 1f; // Duration of the fade effect
    public float waitDuration = 1f; // Duration to wait before starting the fade

    private void Start()
    {
        fadeImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        GameManager.OnTransition += Fade;
    }

    private void OnDisable()
    {
        GameManager.OnTransition -= Fade;
    }

    
    public void Fade()
    {
        Sequence fadeSequence = DOTween.Sequence();

        fadeSequence.Append(fadeImage.DOFade(1f, fadeDuration)) // Fade to black
            .AppendInterval(waitDuration) // Wait for a moment
            .Append(fadeImage.DOFade(0f, fadeDuration)) // Fade back to transparent
            .SetEase(Ease.InOutQuad);
    }
}

using System;
using DG.Tweening;
using UnityEngine;
using Yarn.Unity;

public class DiaglogueTransition : MonoBehaviour
{
    public float originalPosition;
    public float target;
    public float transitionDuration = 1.0f;
    public float fadeOutDuration = 0.2f;
    public CanvasGroup canvasGroup;
    
    private RectTransform rectTransform;
    private bool isCollapsing = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        LinePresenter.OnLineDisplayed += HandleLineIntransition;
        WordDisplayManager.OnOptionsDisplayed += HandleLineOuttransition;
        GameTransition.OnCollapseCompleted += HandleCollapseCompleted;
        GameTransition.OnExpandStarted += HandleLineOuttransition;
        GameTransition.OnCollapseStarted += HandleCollapseStarted;
    }

    private void OnDisable()
    {
        LinePresenter.OnLineDisplayed -= HandleLineIntransition;
        WordDisplayManager.OnOptionsDisplayed -= HandleLineOuttransition;
        GameTransition.OnCollapseCompleted -= HandleCollapseCompleted;
        GameTransition.OnExpandStarted -= HandleLineOuttransition;
        GameTransition.OnCollapseStarted -= HandleCollapseStarted;
    }

    private void HandleCollapseStarted()
    {
        isCollapsing = true; // Block new line animations
    }

    private void HandleCollapseCompleted()
    {
        isCollapsing = false; // Allow animations again
    }

    private void HandleLineIntransition()
    {
        if (isCollapsing) return; // Block if still collapsing

        canvasGroup.DOFade(1f, transitionDuration).SetEase(Ease.InSine);
        rectTransform.DOAnchorPosY(originalPosition, transitionDuration).SetEase(Ease.OutSine);
    }

    private void HandleLineOuttransition()
    {
        canvasGroup.DOFade(0f, fadeOutDuration).SetEase(Ease.InSine);
        rectTransform.DOAnchorPosY(target, transitionDuration).SetEase(Ease.OutSine);
    }
}

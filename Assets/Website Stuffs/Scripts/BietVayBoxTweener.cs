using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class RectSizeTweener : MonoBehaviour
{
    [Header("Target Size")]
    public Vector2 targetSize = new Vector2(800, 800);

    [Header("Tween Settings")]
    [Tooltip("Time it takes to complete the tween (seconds).")]
    public float duration = 0.5f;
    public Ease easeType = Ease.OutQuad;

    [Header("Animator Triggers (this GameObject's Animator)")]
    public Animator animator;
    [Tooltip("Trigger to show the FRONT view (fired at the START of shrink).")]
    public string triggerShowFront = "ShowFront";
    [Tooltip("Trigger to show the BACK view (fired at the START of expand).")]
    public string triggerShowBack = "ShowBack";

    private RectTransform rectTransform;
    private Tween sizeTween;
    private Vector2 originalSize;
    private bool isExpanded = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (!animator) animator = GetComponent<Animator>();
        originalSize = rectTransform.sizeDelta;
    }

    [ContextMenu("Expand")]
    public void PlayTween() => Expand();

    [ContextMenu("Shrink")]
    public void PlayShrink() => Shrink();

    [ContextMenu("Reset Size")]
    public void ResetSize()
    {
        KillTween();
        rectTransform.sizeDelta = originalSize;
        isExpanded = false;
        TryTrigger(animator, triggerShowFront); // front on reset
    }

    /// <summary>Toggle: expand if collapsed, shrink if expanded.</summary>
    public void ReleasedBietVay()
    {
        if (isExpanded) Shrink();
        else Expand();
    }

    private void Expand()
    {
        if (isExpanded) return;

        KillTween();

        // Fire BACK view at the START of expand
        TryTrigger(animator, triggerShowBack);

        sizeTween = rectTransform.DOSizeDelta(targetSize, duration)
            .SetEase(easeType)
            .OnComplete(() =>
            {
                isExpanded = true;
            });
    }

    public void Shrink()
    {
        if (!isExpanded) return;

        KillTween();

        // Fire FRONT view at the START of shrink
        TryTrigger(animator, triggerShowFront);

        sizeTween = rectTransform.DOSizeDelta(originalSize, duration)
            .SetEase(easeType)
            .OnComplete(() =>
            {
                isExpanded = false;
            });
    }

    private void KillTween()
    {
        if (sizeTween != null && sizeTween.IsActive())
        {
            sizeTween.Kill();
            sizeTween = null;
        }
    }

    private static void TryTrigger(Animator anim, string trigger)
    {
        if (!anim || string.IsNullOrEmpty(trigger)) return;
        anim.ResetTrigger(trigger);
        anim.SetTrigger(trigger);
    }
}

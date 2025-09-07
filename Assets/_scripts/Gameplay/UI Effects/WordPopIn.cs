using UnityEngine;
using DG.Tweening;

public class WordPopIn : MonoBehaviour
{
    [Header("Pop Settings")]
    public float popDuration = 0.3f;
    public float popOvershoot = 1.2f;
    public Ease popEase = Ease.OutBack;

    [Header("Delay")]
    public float startDelay = 0.5f; // seconds to wait before pop

    private void OnEnable()
    {
        // Reset scale to 0 before animating
        transform.localScale = Vector3.zero;

        // Create tween
        var tween = transform.DOScale(Vector3.one, popDuration)
            .SetEase(popEase)
            .SetDelay(startDelay); // wait before starting

        // If using Back easing, apply overshoot
        if (popEase == Ease.OutBack || popEase == Ease.InOutBack || popEase == Ease.InBack)
        {
            tween.SetEase(popEase, popOvershoot);
        }
    }
}
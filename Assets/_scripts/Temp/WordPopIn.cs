using UnityEngine;
using DG.Tweening;

public class WordPopIn : MonoBehaviour
{
    public float popDuration = 0.3f;
    public float popOvershoot = 1.2f;
    public Ease popEase = Ease.OutBack; // Exposed in Inspector

    private void OnEnable()
    {
        // Reset scale to 0 before animating
        transform.localScale = Vector3.zero;

        // Tween to full scale using the chosen ease
        var tween = transform.DOScale(Vector3.one, popDuration)
            .SetEase(popEase);

        // If using OutBack/Elastic, apply overshoot
        if (popEase == Ease.OutBack || popEase == Ease.InOutBack || popEase == Ease.InBack)
        {
            tween.SetEase(popEase, popOvershoot);
        }
    }
}
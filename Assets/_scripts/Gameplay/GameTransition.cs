using UnityEngine;
using DG.Tweening;

public class GameTransition : MonoBehaviour
{
    public RectTransform targetRect;
    public Vector2 startSize = Vector2.zero;     // Collapsed size
    public Vector2 endSize = new Vector2(500, 300); // Full expanded size
    public float transitionDuration = 0.5f;

    void Start()
    {
        targetRect.sizeDelta = startSize;
    }

    public void Expand()
    {
        targetRect.DOSizeDelta(endSize, transitionDuration).SetEase(Ease.InOutSine);
    }

    public void Collapse()
    {
        targetRect.DOSizeDelta(startSize, transitionDuration).SetEase(Ease.InOutSine);
    }
}
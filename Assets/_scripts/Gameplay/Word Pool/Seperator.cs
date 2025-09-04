using UnityEngine;
using DG.Tweening;

public class Separator : MonoBehaviour
{
    public RectTransform targetRect;
    public float duration = 0.5f;

    private Vector2 defaultSize;

    void Awake()
    {
        if (targetRect == null)
            targetRect = GetComponent<RectTransform>();

        defaultSize = targetRect.sizeDelta;
    }

    void OnEnable()
    {
        targetRect.sizeDelta = Vector2.zero;
        targetRect.DOSizeDelta(defaultSize, duration).SetEase(Ease.OutQuad);
    }
}
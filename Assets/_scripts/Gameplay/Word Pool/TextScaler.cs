using TMPro;
using UnityEngine;
using DG.Tweening;   // 👈 DOTween namespace

public class TextScaler : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private RectTransform rectTransform;

    private float preferredWidth;
    private float preferredHeight;

    [Header("Default Padding")]
    [SerializeField] private float horizontalPadding = 28f;
    [SerializeField] private float verticalPadding = 7f;

    [Header("Visual Mode Padding")]
    public bool isVisual = false;
    [SerializeField] private float visualHorizontalPadding = 24f;
    [SerializeField] private float visualVerticalPadding = 7f;
    [SerializeField] private float visualPaddingOverlapped = 14.4f;

    [Tooltip("When true, use visual padding instead of default padding.")]
    public bool isSpacingTextScaler = false;

    [Header("Animation Settings")]
    [SerializeField] private float tweenDuration = 0.25f;
    [SerializeField] private Ease tweenEase = Ease.OutQuad;

    // active tweens
    private Tween widthTween;
    private Tween heightTween;

    private void Awake()
    {
        if (!text) text = GetComponent<TMP_Text>();
        if (!rectTransform && text) rectTransform = text.GetComponent<RectTransform>();
    }

    private void Update()
    {
        float currentHorizontalPadding = isSpacingTextScaler
            ? visualPaddingOverlapped
            : (isVisual ? visualHorizontalPadding : horizontalPadding);

        float currentVerticalPadding = isSpacingTextScaler
            ? visualVerticalPadding
            : verticalPadding;

        CalculatePreferredSize();
        ScaleWithPreferredSize(currentHorizontalPadding, currentVerticalPadding);
    }

    private void ScaleWithPreferredSize(float hPad, float vPad)
    {
        if (!rectTransform) return;

        float targetWidth = Mathf.CeilToInt(preferredWidth) + hPad;
        float targetHeight = Mathf.CeilToInt(preferredHeight) + vPad;

        // Kill old tweens so they don’t overlap
        widthTween?.Kill();
        heightTween?.Kill();

        // Animate floats separately
        widthTween = DOTween.To(
            () => rectTransform.sizeDelta.x,
            x => rectTransform.sizeDelta = new Vector2(x, rectTransform.sizeDelta.y),
            targetWidth,
            tweenDuration
        ).SetEase(tweenEase);

        heightTween = DOTween.To(
            () => rectTransform.sizeDelta.y,
            y => rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, y),
            targetHeight,
            tweenDuration
        ).SetEase(tweenEase);
    }

    private void CalculatePreferredSize()
    {
        if (!text) return;
        preferredWidth = text.preferredWidth;
        preferredHeight = text.preferredHeight;
    }
}

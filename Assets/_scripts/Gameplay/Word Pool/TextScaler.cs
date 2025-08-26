using TMPro;
using UnityEngine;

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

    [Header("Lerp Settings")]
    [SerializeField] private float lerpSpeed = 10f; // Higher = snappier

    private Vector2 targetSize;

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
        SetTargetSize(currentHorizontalPadding, currentVerticalPadding);
        SmoothResize();
    }

    private void SetTargetSize(float hPad, float vPad)
    {
        targetSize = new Vector2(
            Mathf.CeilToInt(preferredWidth) + hPad,
            Mathf.CeilToInt(preferredHeight) + vPad
        );
    }

    private void SmoothResize()
    {
        if (!rectTransform) return;

        // Smoothly interpolate between current size and target size
        rectTransform.sizeDelta = Vector2.Lerp(
            rectTransform.sizeDelta,
            targetSize,
            Time.deltaTime * lerpSpeed
        );
    }

    private void CalculatePreferredSize()
    {
        if (!text) return;
        preferredWidth = text.preferredWidth;
        preferredHeight = text.preferredHeight;
    }
}

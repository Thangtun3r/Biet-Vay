using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // <-- needed for Image

public class WordVisualInteraction : MonoBehaviour
{
    [Header("Target")]
    public RectTransform ButtonRectTransform;
    public RectTransform WrapperButtonRect;
    public GameObject buttonShadow;

    [Header("Hover Motion")]
    public float hoverOffsetY = 4f;

    [Tooltip("Hover/exit tween duration.")]
    public float tweenDuration = 0.2f;

    private float baseY;
    private bool isHovering;
    private Tween currentTween;

    // cache image reference
    private Image buttonShadowImage;

    // --- Lifecycle ---
    private void Awake()
    {
        if (ButtonRectTransform == null)
            ButtonRectTransform = GetComponent<RectTransform>();

        if (buttonShadow != null)
            buttonShadowImage = buttonShadow.GetComponent<Image>();
    }

    private void OnEnable()
    {
        isHovering = false;

        if (ButtonRectTransform != null)
        {
            baseY = ButtonRectTransform.anchoredPosition.y;
            ButtonRectTransform.anchoredPosition = new Vector2(
                ButtonRectTransform.anchoredPosition.x, baseY);
        }

        if (buttonShadowImage != null)
            buttonShadowImage.enabled = false; // start hidden
    }

    private void OnDisable()
    {
        isHovering = false;

        KillCurrentTween();
        if (ButtonRectTransform != null)
        {
            ButtonRectTransform.DOKill();
            ButtonRectTransform.anchoredPosition = new Vector2(
                ButtonRectTransform.anchoredPosition.x, baseY);
            ButtonRectTransform.localScale = Vector3.one;
        }
    }

    // --- Public hooks ---
    public void HandleHoverVisual(PointerEventData _)
    {
        if (ButtonRectTransform == null) return;
        if (isHovering) return;

        isHovering = true;

        KillCurrentTween();
        ButtonRectTransform.DOKill();

        float targetY = baseY + hoverOffsetY;

        currentTween = ButtonRectTransform
            .DOAnchorPosY(targetY, tweenDuration)
            .SetEase(Ease.OutSine)
            .OnComplete(() => currentTween = null);
    }

    public void HandleExitVisual(PointerEventData _)
    {
        if (ButtonRectTransform == null) return;
        isHovering = false;

        KillCurrentTween();
        ButtonRectTransform.DOKill();

        currentTween = ButtonRectTransform
            .DOAnchorPosY(baseY, tweenDuration)
            .SetEase(Ease.OutSine)
            .OnComplete(() => currentTween = null);
    }

    public void HandleBeginDragVisual(PointerEventData _)
    {
        HandleExitVisual(_);

        if (buttonShadowImage != null)
            buttonShadowImage.enabled = true;

        Vector3 targetPos = new Vector3(0f, -4f, 0f);
        buttonShadow.transform
            .DOLocalMove(targetPos, 0.1f)
            .SetEase(Ease.OutSine);

        if (WrapperButtonRect != null)
        {
            WrapperButtonRect.DOKill(true);

            Sequence wiggle = DOTween.Sequence();
            wiggle.Append(WrapperButtonRect.DOLocalRotate(
                    new Vector3(0, 0, 8f), 0.04f))
                .Append(WrapperButtonRect.DOLocalRotate(
                    new Vector3(0, 0, -5f), 0.16f))
                .Append(WrapperButtonRect.DOLocalRotate(
                    Vector3.zero, 0.08f))
                .SetEase(Ease.InOutSine);
        }
    }

    public void HandleDragVisual(PointerEventData _)
    {
        transform.SetAsLastSibling();
        HandleExitVisual(_);
    }

    public void HandleEndDragVisual(PointerEventData _)
    {
        Vector3 targetPos = new Vector3(0f, 0f, 0);

        buttonShadow.transform
            .DOLocalMove(targetPos, 0.1f)
            .SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                if (buttonShadowImage != null)
                    buttonShadowImage.enabled = false;
            });
    }

    private void KillCurrentTween()
    {
        if (currentTween != null)
        {
            currentTween.Kill();
            currentTween = null;
        }
    }
}

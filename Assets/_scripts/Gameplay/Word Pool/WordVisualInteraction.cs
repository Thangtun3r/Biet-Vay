using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class WordVisualInteraction : MonoBehaviour
{
    public RectTransform ButtonRectTransform;

    [Tooltip("How far to nudge on hover, in anchored Y units.")]
    public float hoverOffsetY = 1.5f;

    [Tooltip("Hover/exit tween duration.")]
    public float tweenDuration = 0.1f;

    private float baseY;              // stable baseline
    private bool hasBaseline;         // did we capture baseline?
    private bool isHovering;          // debounce repeated enters
    private Tween currentTween;       // keep a handle to the running tween

    private void Awake()
    {
        if (ButtonRectTransform == null)
            ButtonRectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        // Capture baseline once at enable.
        if (ButtonRectTransform != null)
        {
            baseY = ButtonRectTransform.anchoredPosition.y;
            hasBaseline = true;
        }
    }

    // Optional: if layout moves us while NOT hovering, refresh the baseline.
    private void LateUpdate()
    {
        if (!hasBaseline || isHovering || ButtonRectTransform == null) return;

        // If no tween is playing and layout shifted our Y, adopt the new rest position.
        if (currentTween == null || !currentTween.IsActive() || !currentTween.IsPlaying())
        {
            baseY = ButtonRectTransform.anchoredPosition.y;
        }
    }

    public void HandleDragVisual(PointerEventData _)
    {
        transform.SetAsLastSibling();
        HandleExitVisual(_);
    }
    
    public void HandleBeginDragVisual(PointerEventData _)
    {
        
    }

    public void HandleHoverVisual(PointerEventData _)
    {
        if (ButtonRectTransform == null) return;
        if (isHovering) return; // debounce extra enters over children

        isHovering = true;

        // Kill any running tween on this target
        currentTween?.Kill();
        ButtonRectTransform.DOKill();

        // Always tween relative to the STABLE baseline
        float targetY = baseY + hoverOffsetY;

        currentTween = ButtonRectTransform
            .DOAnchorPosY(targetY, tweenDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => { currentTween = null; });
    }

    public void HandleExitVisual(PointerEventData _)
    {
        if (ButtonRectTransform == null) return;
        if (!isHovering) return; // debounce duplicate exits

        isHovering = false;

        currentTween?.Kill();
        ButtonRectTransform.DOKill();

        currentTween = ButtonRectTransform
            .DOAnchorPosY(baseY, tweenDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => { currentTween = null; });
    }

    private void OnDisable()
    {
        currentTween?.Kill();
        currentTween = null;
        if (ButtonRectTransform != null) ButtonRectTransform.DOKill();
    }
}

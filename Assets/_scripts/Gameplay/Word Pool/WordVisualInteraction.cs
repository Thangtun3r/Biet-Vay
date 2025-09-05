using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class WordVisualInteraction : MonoBehaviour
{
    public RectTransform ButtonRectTransform;

    [Tooltip("How far to nudge on hover, in anchored Y units.")]
    public float hoverOffsetY = 4f;

    [Tooltip("Hover/exit tween duration.")]
    public float tweenDuration = 0.2f;

    [Tooltip("Pixels of movement before we accept a new baseline.")]
    public float baselineEpsilon = 1f;

    [Tooltip("Seconds to wait after a tween ends/kills before baseline can refresh.")]
    public float baselineCooldown = 0.2f;

    private float baseY;               // stable baseline
    private bool hasBaseline;          // did we capture baseline?
    private bool isHovering;           // debounce repeated enters
    private Tween currentTween;        // handle to running tween
    private float lastTweenEndTime;    // for cooldown after tween end/kill

    private void Awake()
    {
        if (ButtonRectTransform == null)
            ButtonRectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        // Reset hover state on enable to avoid stale 'true' blocking handlers
        isHovering = false;

        // Capture baseline once at enable (if not already captured)
        if (ButtonRectTransform != null)
        {
            if (!hasBaseline)
            {
                baseY = ButtonRectTransform.anchoredPosition.y;
                hasBaseline = true;
            }
            else
            {
                // Snap to known baseline on enable (optional but keeps things sane)
                ButtonRectTransform.anchoredPosition = new Vector2(
                    ButtonRectTransform.anchoredPosition.x, baseY);
            }
        }
    }

    private void LateUpdate()
    {
        if (!hasBaseline || isHovering || ButtonRectTransform == null) return;

        bool tweenPlaying = currentTween != null && currentTween.IsActive() && currentTween.IsPlaying();
        if (tweenPlaying) return;

        // Respect a small cooldown after any tween end/kill to avoid learning mid-flight values
        if (Time.unscaledTime - lastTweenEndTime < baselineCooldown) return;

        float currentY = ButtonRectTransform.anchoredPosition.y;
        if (Mathf.Abs(currentY - baseY) > baselineEpsilon)
        {
            baseY = currentY;
        }
    }

    public void HandleDragVisual(PointerEventData _)
    {
        transform.SetAsLastSibling();
        HandleExitVisual(_);
    }

    public void HandleBeginDragVisual(PointerEventData _) { }

    public void HandleHoverVisual(PointerEventData _)
    {
        if (ButtonRectTransform == null) return;
        if (isHovering) return; // debounce

        isHovering = true;

        KillCurrentTween();
        ButtonRectTransform.DOKill(); // kill any leftover tweens on the target

        float targetY = baseY + hoverOffsetY;

        currentTween = ButtonRectTransform
            .DOAnchorPosY(targetY, tweenDuration)
            .SetEase(Ease.OutSine)
            .OnComplete(() => { currentTween = null; lastTweenEndTime = Time.unscaledTime; });
    }

    public void HandleExitVisual(PointerEventData _)
    {
        if (ButtonRectTransform == null) return;
        if (!isHovering) return; // debounce duplicate exits

        isHovering = false;

        KillCurrentTween();
        ButtonRectTransform.DOKill();

        currentTween = ButtonRectTransform
            .DOAnchorPosY(baseY, tweenDuration)
            .SetEase(Ease.OutSine)
            .OnComplete(() => { currentTween = null; lastTweenEndTime = Time.unscaledTime; });
    }

    private void OnDisable()
    {
        // Make sure state and visuals are fully reset
        isHovering = false;

        KillCurrentTween();
        if (ButtonRectTransform != null)
        {
            ButtonRectTransform.DOKill(); // don't complete; we will snap ourselves
            ButtonRectTransform.anchoredPosition = new Vector2(
                ButtonRectTransform.anchoredPosition.x, baseY);
            ButtonRectTransform.localScale = Vector3.one;
        }

        lastTweenEndTime = Time.unscaledTime;
    }

    private void KillCurrentTween()
    {
        if (currentTween != null)
        {
            // Kill without completing; we want to start next tween from current value
            currentTween.Kill();
            currentTween = null;
            lastTweenEndTime = Time.unscaledTime;
        }
    }
}

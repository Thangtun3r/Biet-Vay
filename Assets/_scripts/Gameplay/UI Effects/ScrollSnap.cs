using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening; // DOTween

[RequireComponent(typeof(ScrollRect))]
public class ScrollSnapToTarget : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public enum VerticalAlign { Top, Center, Bottom }
    public enum HorizontalAlign { Left, Center, Right }

    [Header("What to snap to")]
    public RectTransform target;

    [Header("When to snap")]
    [Tooltip("Snap only if the target's center is within this many pixels of the chosen alignment point.")]
    public float snapThresholdPixels = 140f;

    [Header("How to align on snap")]
    public bool vertical = true;
    public VerticalAlign verticalAlign = VerticalAlign.Center;
    public bool horizontal = false;
    public HorizontalAlign horizontalAlign = HorizontalAlign.Center;

    [Header("Motion")]
    public float tweenDuration = 0.22f;
    public Ease ease = Ease.OutCubic;
    public bool disableInertiaDuringSnap = true;
    public bool useUnscaledTime = false;

    [Header("Locking")]
    [Tooltip("If true, disables ScrollRect interaction after snapping until Unlock() is called.")]
    public bool lockAfterSnap = false;

    private ScrollRect sr;
    private RectTransform content;
    private RectTransform viewport;
    private Tween activeTween;

    void Awake()
    {
        sr = GetComponent<ScrollRect>();
        content = sr.content;
        viewport = sr.viewport ? sr.viewport : GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData _)
    {
        if (sr == null || !sr.enabled) return; // ignore if locked
        KillTween();
    }

    public void OnEndDrag(PointerEventData _)
    {
        if (sr == null || !sr.enabled) return; // ignore if locked
        if (target == null) return;

        Vector2 targetLocalInViewport = WorldToViewportLocal(target);
        Vector2 alignPointLocal = GetAlignmentPointLocal();
        Vector2 deltaToAlign = alignPointLocal - targetLocalInViewport;

        float dist = 0f;
        if (vertical && !horizontal) dist = Mathf.Abs(deltaToAlign.y);
        else if (horizontal && !vertical) dist = Mathf.Abs(deltaToAlign.x);
        else dist = deltaToAlign.magnitude;

        if (dist <= snapThresholdPixels)
        {
            Vector2 requiredContentShift = ViewportDeltaToContentDelta(deltaToAlign);
            Vector2 targetAnchoredPos = content.anchoredPosition - requiredContentShift;
            StartTweenTo(targetAnchoredPos);
        }
    }

    // --- DOTween snap ---

    void StartTweenTo(Vector2 targetAnchored)
    {
        KillTween();

        if (disableInertiaDuringSnap) sr.inertia = false;
        sr.velocity = Vector2.zero;

        activeTween = content.DOAnchorPos(targetAnchored, Mathf.Max(0.0001f, tweenDuration))
            .SetEase(ease)
            .SetUpdate(useUnscaledTime)
            .SetLink(gameObject, LinkBehaviour.KillOnDisable)
            .OnComplete(() =>
            {
                if (disableInertiaDuringSnap) sr.inertia = true;
                if (lockAfterSnap) sr.enabled = false; // lock interaction
                activeTween = null;
            });
    }

    void KillTween()
    {
        if (activeTween != null && activeTween.IsActive())
        {
            activeTween.Kill();
            activeTween = null;
        }
        if (disableInertiaDuringSnap) sr.inertia = true;
    }

    // Unlock ScrollRect externally
    public void Unlock()
    {
        sr.enabled = true;
    }

    // --- Helpers ---

    Vector2 WorldToViewportLocal(RectTransform rt)
    {
        Vector3 worldCenter = rt.TransformPoint(rt.rect.center);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            viewport, RectTransformUtility.WorldToScreenPoint(null, worldCenter),
            null, out var local);
        return local;
    }

    Vector2 GetAlignmentPointLocal()
    {
        Vector2 p = Vector2.zero;
        Vector2 size = viewport.rect.size;

        if (vertical)
        {
            switch (verticalAlign)
            {
                case VerticalAlign.Top:    p.y =  size.y * 0.5f; break;
                case VerticalAlign.Center: p.y =  0f;            break;
                case VerticalAlign.Bottom: p.y = -size.y * 0.5f; break;
            }
        }
        if (horizontal)
        {
            switch (horizontalAlign)
            {
                case HorizontalAlign.Left:   p.x = -size.x * 0.5f; break;
                case HorizontalAlign.Center: p.x =  0f;             break;
                case HorizontalAlign.Right:  p.x =  size.x * 0.5f;  break;
            }
        }
        return p;
    }

    Vector2 ViewportDeltaToContentDelta(Vector2 viewportDelta)
    {
        return new Vector2(
            horizontal ? viewportDelta.x : 0f,
            vertical ? -viewportDelta.y : 0f
        );
    }
}

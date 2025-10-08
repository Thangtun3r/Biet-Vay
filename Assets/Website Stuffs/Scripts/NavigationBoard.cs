using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UIWhiteboardInertiaPC : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
    [Tooltip("Leave empty to use this RectTransform.")]
    public RectTransform target;

    [Header("Zoom")]
    [Min(0.01f)] public float minZoom = 0.25f;
    [Min(0.01f)] public float maxZoom = 5f;
    [Range(0.01f, 1f)] public float zoomStep = 0.1f;   // wheel sensitivity
    [Min(0f)] public float zoomSmooth = 0.1f;          // 0 = snap; >0 = smooth
    public bool zoomToPointer = true;

    [Header("Pan Inertia")]
    [Range(0f, 1f)] public float deceleration = 0.1f;  // higher = stops sooner
    public float maxSpeed = 4000f;

    private RectTransform rt, parentRect;
    private bool dragging;
    private Vector2 dragStart, rectStart;
    private Vector2 velocity;
    private Vector2 lastMousePos;

    private float curScale, targetScale, zoomVel;

    // --- NEW: remember pivot while smoothing so the point under the mouse stays fixed ---
    private Vector2? zoomPivotParentPoint = null;

    void Awake()
    {
        rt = target ? target : GetComponent<RectTransform>();
        parentRect = rt.parent as RectTransform;
        curScale = targetScale = rt.localScale.x;
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        if (dt <= 0f) return;

        // --- Smooth zoom with per-frame pivot lock ---
        float prevScale = curScale;
        curScale = (zoomSmooth <= 0f)
            ? targetScale
            : Mathf.SmoothDamp(curScale, targetScale, ref zoomVel, zoomSmooth, Mathf.Infinity, dt);

        if (!Mathf.Approximately(curScale, prevScale))
        {
            float ratio = (prevScale > 0f) ? (curScale / prevScale) : 1f;

            if (zoomPivotParentPoint.HasValue && parentRect != null && !Mathf.Approximately(ratio, 1f))
            {
                Vector2 p = zoomPivotParentPoint.Value;
                Vector2 anchored = rt.anchoredPosition + (p - rt.anchoredPosition) * (1f - ratio);
                rt.anchoredPosition = ClampToParent(anchored);
            }

            rt.localScale = new Vector3(curScale, curScale, 1f);

            // When we finish smoothing, clear pivot + hard-clamp
            if (Mathf.Abs(targetScale - curScale) < 0.0005f && Mathf.Abs(zoomVel) < 0.01f)
            {
                curScale = targetScale;
                zoomVel = 0f;
                zoomPivotParentPoint = null;
                rt.anchoredPosition = ClampToParent(rt.anchoredPosition);
            }
        }

        // Inertia move after releasing drag
        if (!dragging && velocity.sqrMagnitude > 0.001f)
        {
            rt.anchoredPosition = ClampToParent(rt.anchoredPosition + velocity * dt);
            velocity *= Mathf.Pow(1f - deceleration, dt * 60f); // exponential decay ~60fps normalized
        }
    }

    // --- Dragging ---
    public void OnBeginDrag(PointerEventData e)
    {
        if (parentRect == null) return;

        dragging = true;
        velocity = Vector2.zero;
        lastMousePos = e.position;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, e.position, e.pressEventCamera, out dragStart);
        rectStart = rt.anchoredPosition;
    }

    public void OnDrag(PointerEventData e)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, e.position, e.pressEventCamera, out var cur)) return;

        Vector2 delta = cur - dragStart;
        rt.anchoredPosition = ClampToParent(rectStart + delta);

        // estimate velocity for inertia
        float dt = Mathf.Max(Time.unscaledDeltaTime, 1e-5f);
        Vector2 mouseDelta = (Vector2)e.position - lastMousePos;
        velocity = Vector2.Lerp(velocity, mouseDelta / dt, 0.6f);
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
        lastMousePos = e.position;
    }

    public void OnEndDrag(PointerEventData e) => dragging = false;

    // --- Wheel Zoom ---
    public void OnScroll(PointerEventData e)
    {
        float delta = e.scrollDelta.y;
        if (Mathf.Approximately(delta, 0f)) return;

        float newScale = Mathf.Clamp(curScale * Mathf.Pow(1f + zoomStep, delta), minZoom, maxZoom);

        // Store pivot so it stays fixed through the whole smoothing phase
        if (zoomToPointer && parentRect != null &&
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, e.position, e.pressEventCamera, out var pivot))
        {
            zoomPivotParentPoint = pivot;
        }
        else
        {
            zoomPivotParentPoint = null;
        }

        // If no smoothing, adjust immediately
        if (zoomSmooth <= 0f && zoomPivotParentPoint.HasValue)
        {
            float ratio = (curScale > 0f) ? newScale / curScale : 1f;
            Vector2 anchored = rt.anchoredPosition + (zoomPivotParentPoint.Value - rt.anchoredPosition) * (1f - ratio);
            rt.anchoredPosition = ClampToParent(anchored);
        }

        targetScale = newScale;
    }

    // --- Bounds clamp ---
    private Vector2 ClampToParent(Vector2 pos)
    {
        if (parentRect == null) return pos;

        Vector2 img = new Vector2(rt.rect.width * rt.lossyScale.x, rt.rect.height * rt.lossyScale.y);
        Vector2 par = new Vector2(parentRect.rect.width * parentRect.lossyScale.x, parentRect.rect.height * parentRect.lossyScale.y);

        if (img.x <= par.x) pos.x = 0f;
        else
        {
            float limX = (img.x - par.x) * 0.5f;
            pos.x = Mathf.Clamp(pos.x, -limX, limX);
        }

        if (img.y <= par.y) pos.y = 0f;
        else
        {
            float limY = (img.y - par.y) * 0.5f;
            pos.y = Mathf.Clamp(pos.y, -limY, limY);
        }

        return pos;
    }
}

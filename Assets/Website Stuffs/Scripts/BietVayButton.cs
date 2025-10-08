using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(BoxCollider2D))]
public class BietVayWebButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerDownHandler, IPointerUpHandler
{
    public enum InteractionState { Idle, Hover, Pressed, Dragging }

    public RectSizeTweener bietVayBoxTweener;
    [Header("References")]
    public RectTransform buttonVisual;
    public RectTransform shadow;
    public RectTransform dragTarget;
    public Canvas canvas;

    [Tooltip("Optional: another object to disable together with this one.")]
    public GameObject theButton;

    [Header("Motion Amounts (in local Y units)")]
    public float hoverRaise = 12f;
    public float shadowDrop = 8f;

    [Header("Tweening")]
    public float tweenTime = 0.08f;
    public Ease tweenEase = Ease.OutQuad;

    [Header("Disable Settings")]
    [Tooltip("When exiting a trigger with this tag, objects will disable *after release*.")]
    public string allowedTag = "DropZone";

    [Tooltip("Seconds it takes to snap back to original position.")]
    public float snapBackTime = 0.3f;

    [Tooltip("Delay before disabling objects after exiting the trigger.")]
    public float disableDelay = 0.2f;

    // internals
    private RectTransform _self;
    private BoxCollider2D _collider;
    private bool _isPointerOver = false;
    private bool _isDragging = false;
    private bool _isPointerDown = false;
    private bool shouldDisableOnRelease = false;

    private Vector2 _visualBasePos;
    private Vector2 _shadowBasePos;
    private Vector2 _dragBasePos; // for snapback reference

    private Tween _visualTween;
    private Tween _shadowTween;
    private Tween _snapTween;

    private bool snappingEnabled = true;
    private Coroutine _disableCo;

    public InteractionState CurrentState { get; private set; } = InteractionState.Idle;

    private void Awake()
    {
        _self = GetComponent<RectTransform>();
        _collider = GetComponent<BoxCollider2D>();

        if (dragTarget == null) dragTarget = _self;
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        snappingEnabled = true;
        shouldDisableOnRelease = false;
        _disableCo = null;
    }

    private void Start()
    {
        if (buttonVisual) _visualBasePos = buttonVisual.anchoredPosition;
        if (shadow)       _shadowBasePos = shadow.anchoredPosition;
        _dragBasePos = dragTarget.anchoredPosition;

        SetState(InteractionState.Idle);
    }

    private void OnDisable()
    {
        KillTweens();
        if (buttonVisual) buttonVisual.anchoredPosition = _visualBasePos;
        if (shadow)       shadow.anchoredPosition = _shadowBasePos;
        if (dragTarget)   dragTarget.anchoredPosition = _dragBasePos;

        _isDragging = _isPointerDown = _isPointerOver = false;
        SetState(InteractionState.Idle);
    }

    // -------------------- Pointer & Drag --------------------

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isPointerOver = true;
        if (!_isDragging && !_isPointerDown) SetState(InteractionState.Hover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerOver = false;
        if (!_isDragging && !_isPointerDown) SetState(InteractionState.Idle);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPointerDown = true;
        if (!_isDragging) SetState(InteractionState.Pressed);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPointerDown = false;
        if (!_isDragging)
        {
            SetState(_isPointerOver ? InteractionState.Hover : InteractionState.Idle);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        _dragBasePos = dragTarget.anchoredPosition; // remember for snap back
        SetState(InteractionState.Dragging);
        _snapTween?.Kill(); // cancel any ongoing snap so drag feels responsive
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragTarget == null) return;

        float scale = 1f;
        if (canvas && canvas.renderMode != RenderMode.WorldSpace && canvas.scaleFactor != 0)
            scale = canvas.scaleFactor;

        dragTarget.anchoredPosition += eventData.delta / scale;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;

        // Always snap back visually
        if (snappingEnabled)
            SnapBack();

        SetState(_isPointerOver ? InteractionState.Hover : InteractionState.Idle);

        // Disable only after release if previously exited trigger
        if (shouldDisableOnRelease)
            StopSnappingAndDisableDelayed();
    }

    // -------------------- Collider trigger logic --------------------

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(allowedTag))
        {
            // Mark for disable after drag release
            shouldDisableOnRelease = true;
        }
    }

    // -------------------- Visual State Handling --------------------

    private void SetState(InteractionState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;

        switch (CurrentState)
        {
            case InteractionState.Idle:
                RaiseVisual(false);
                RaiseShadow(false);
                break;

            case InteractionState.Hover:
                RaiseVisual(true);
                RaiseShadow(false);
                break;

            case InteractionState.Pressed:
                RaiseVisual(false);
                RaiseShadow(false);
                break;

            case InteractionState.Dragging:
                RaiseVisual(false);
                RaiseShadow(true);
                break;
        }
    }

    private void RaiseVisual(bool raised)
    {
        if (!buttonVisual) return;

        Vector2 target = _visualBasePos + (raised ? new Vector2(0f, hoverRaise) : Vector2.zero);
        _visualTween?.Kill();
        _visualTween = buttonVisual.DOAnchorPos(target, tweenTime).SetEase(tweenEase);
    }

    private void RaiseShadow(bool dropped)
    {
        if (!shadow) return;

        Vector2 target = _shadowBasePos + (dropped ? new Vector2(0f, -shadowDrop) : Vector2.zero);
        _shadowTween?.Kill();
        _shadowTween = shadow.DOAnchorPos(target, tweenTime).SetEase(tweenEase);
    }

    private void KillTweens()
    {
        _visualTween?.Kill(); _visualTween = null;
        _shadowTween?.Kill(); _shadowTween = null;
        _snapTween?.Kill();   _snapTween = null;
    }

    // -------------------- Snap control --------------------

    private void SnapBack()
    {
        if (dragTarget == null || !snappingEnabled) return;

        _snapTween?.Kill();
        _snapTween = dragTarget.DOAnchorPos(_dragBasePos, snapBackTime)
            .SetEase(Ease.OutQuad);
    }

    /// <summary>
    /// Stops snapping and disables after a short delay (called on release if exited trigger).
    /// </summary>
    private void StopSnappingAndDisableDelayed()
    {
        if (!snappingEnabled) return;

        snappingEnabled = false;
        KillTweens();

        if (_disableCo == null)
            _disableCo = StartCoroutine(DisableAfterDelay());
    }

    private System.Collections.IEnumerator DisableAfterDelay()
    {
        if (disableDelay > 0f)
            yield return new WaitForSeconds(disableDelay);

        bietVayBoxTweener.ReleasedBietVay();
        if (theButton) theButton.SetActive(false);
        gameObject.SetActive(false);
    }
}

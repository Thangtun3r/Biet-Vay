//Hey we got a bug where the buttonVisual moves up and down when dragging the wordVisual
//This script freezes the buttonVisual's y position to -9f when dragging the wordVisual
//Very hardcoded but it works for now :(
using UnityEngine;

[DefaultExecutionOrder(2000)] // run late so we override earlier movement
public class ButtonVisualYFreeze : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The RectTransform to freeze vertically (your Button Visual).")]
    public RectTransform buttonVisual;

    [Header("Freeze Settings")]
    [Tooltip("Anchored Y to enforce while dragging (Inspector Pos Y).")]
    public float frozenAnchoredY = -9f;

    [Tooltip("If true, read the starting anchored Y at Awake instead of using frozenAnchoredY.")]
    public bool useInitialYFromInspector = false;

    [Tooltip("How long after the last OnDragging ping we still consider dragging (seconds).")]
    public float dragGraceSeconds = 0.05f;

    private float _lastDragPingTime = -999f;

    private void Awake()
    {
        if (useInitialYFromInspector && buttonVisual != null)
        {
            // Capture the value shown in the Inspector at startup
            frozenAnchoredY = buttonVisual.anchoredPosition.y;
        }
    }

    private void OnEnable()
    {
        // Assume this event is fired every frame while dragging (like IDragHandler.OnDrag).
        WordVisualInteraction.OnDragging += OnDraggingPing;
    }

    private void OnDisable()
    {
        WordVisualInteraction.OnDragging -= OnDraggingPing;
    }

    private void OnDraggingPing()
    {
        // Heartbeat that says "dragging is happening *now*"
        _lastDragPingTime = Time.unscaledTime;
    }

    private void LateUpdate()
    {
        if (buttonVisual == null) return;

        // Consider "dragging" true if we recently received a ping.
        bool isDragging = (Time.unscaledTime - _lastDragPingTime) <= dragGraceSeconds;
        if (!isDragging) return;

        // Enforce anchored Y (NOT world position).
        var a = buttonVisual.anchoredPosition;
        a.y = frozenAnchoredY;
        buttonVisual.anchoredPosition = a;
    }
}

using System;
using UnityEngine;
using DG.Tweening; // for ClickShake

public class FrictionMouseFollow : MonoBehaviour
{
    [Header("Follow")] public RectTransform visualButtonWrapper;
    [Min(0f)] public float followSpeed = 5f;
    
    [Header("Distance Threshold")]
    [Min(0f)] public float thresholdDis = 100f;   // UI units (resolution-independent)
    public float currentDistance { get; private set; }

    private VisualWord _visualWord;
    private RectTransform rectTransform;

    // Snapshot (in UI local space)
    private bool _hasSnapshot = false;
    private Vector2 _snapshotAnchoredPos;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        _visualWord = GetComponent<VisualWord>();
    }
    
    


    public void FollowMouse()
    {
        if (_visualWord) _visualWord.enabled = false;

        // Take snapshot once when follow starts
        if (!_hasSnapshot)
        {
            _hasSnapshot = true;
            _snapshotAnchoredPos = rectTransform.anchoredPosition;
        }

        // Convert mouse position to local space
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform.parent as RectTransform,
                Input.mousePosition,
                null, // Overlay canvas
                out var localPoint))
        {
            // Calculate distance from snapshot
            currentDistance = Vector2.Distance(rectTransform.anchoredPosition, _snapshotAnchoredPos);

            // Base follow target = mouse position
            Vector2 target = localPoint;

            // If distance exceeds threshold, add offset
            if (currentDistance > thresholdDis)
            {
                float exceed = currentDistance - thresholdDis;

                // Direction from snapshot to mouse
                Vector2 dir = (localPoint - _snapshotAnchoredPos).normalized;

                // Offset grows with exceed (tweak multiplier as you like)
                Vector2 offset = dir * exceed * 1f;

                target -= offset;
                DragShake();
            }

            // Smoothly move toward target
            rectTransform.anchoredPosition = Vector2.Lerp(
                rectTransform.anchoredPosition,
                target,
                followSpeed * Time.deltaTime
            );
        }
    }


    public void StopFollowMouse()
    {
        if (_visualWord) _visualWord.enabled = true;

        // Reset snapshot to current
        _hasSnapshot = false;
        _snapshotAnchoredPos = rectTransform.anchoredPosition;

        // Distance resets (optional: keep last value if you prefer)
        currentDistance = 0f;
    }

    public void ClickShake()
    {
        rectTransform.DOKill();
        rectTransform.DOShakePosition(
            0.2f,
            strength: 5f,
            vibrato: 20,
            randomness: 90,
            snapping: false,
            fadeOut: true
        );
    }

    public void DragShake()
    {
        
    }
}

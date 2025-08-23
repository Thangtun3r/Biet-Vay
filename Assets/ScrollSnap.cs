using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(ScrollRect))]
public class ScrollSnap : MonoBehaviour
{
    [Header("References")]
    public RectTransform content;               // The ScrollRect content
    public List<RectTransform> items;           // List of target items (images)

    [Header("Settings")]
    public float snapSpeed = 10f;               // How fast it snaps
    public float snapThreshold = 50f;           // Velocity threshold to start snapping

    private ScrollRect _scrollRect;
    private Vector2 _targetPosition;
    private bool _isSnapping = false;

    void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
    }

    void Update()
    {
        if (!_isSnapping)
        {
            // Only snap when not dragging and velocity is low
            if (!Input.GetMouseButton(0) && _scrollRect.velocity.magnitude < snapThreshold)
            {
                SnapToClosest();
            }
        }
        else
        {
            // Smooth snap movement
            content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, _targetPosition, Time.deltaTime * snapSpeed);

            // Stop when close
            if (Vector2.Distance(content.anchoredPosition, _targetPosition) < 0.1f)
            {
                content.anchoredPosition = _targetPosition;
                _isSnapping = false;
            }
        }
    }

    void SnapToClosest()
    {
        float closestDistance = float.MaxValue;
        RectTransform closestItem = null;

        foreach (var item in items)
        {
            float dist = Vector2.Distance(content.anchoredPosition, -item.anchoredPosition);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestItem = item;
            }
        }

        if (closestItem != null)
        {
            _targetPosition = -closestItem.anchoredPosition;
            _isSnapping = true;
        }
    }
}

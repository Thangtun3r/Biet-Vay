using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class RectFollowLerp : MonoBehaviour
{
    [Header("Target to Follow")]
    [Tooltip("The RectTransform to follow.")]
    public RectTransform target;

    [Tooltip("Optional parent canvas to ensure proper coordinate space.")]
    public Canvas canvas;

    [Header("Follow Settings")]
    [Tooltip("How quickly to follow the target (higher = faster).")]
    [Range(0f, 20f)] public float followSpeed = 10f;

    [Tooltip("Offset from the target’s position (in local UI units).")]
    public Vector2 offset;

    [Tooltip("Follow target even if it becomes inactive (debug only).")]
    public bool ignoreInactiveTarget = false;

    private RectTransform self;

    private void Awake()
    {
        self = GetComponent<RectTransform>();
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (target == null) return;
        if (!ignoreInactiveTarget && !target.gameObject.activeInHierarchy) return;

        // Smoothly interpolate position
        Vector2 current = self.anchoredPosition;
        Vector2 desired = target.anchoredPosition + offset;
        Vector2 next = Vector2.Lerp(current, desired, Time.deltaTime * followSpeed);

        self.anchoredPosition = next;
    }
}
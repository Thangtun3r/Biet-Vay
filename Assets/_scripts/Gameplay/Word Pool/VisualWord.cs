using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class VisualWord : MonoBehaviour
{
    [Header("Logic References")]
    public GameObject logicWordObject;
    public Transform target;
    public string logicWordText;

    [Header("Visual References")]
    public TextMeshProUGUI text;
    public float followSpeed = 11f;
    public float horizontalOffset = 0f;

    [Header("Layout / Tween")]
    public GameObject separator;
    public RectTransform seperatorRect;
    public float spacing = 14f;
    public float spacingDuration = 0.25f;
    public float resetDuration = 0.2f;
    public Ease spacingEase = Ease.OutQuad;
    public Ease resetEase = Ease.OutQuad;

    private float originalWidth;
    private Tween widthTween;

    private bool isSpacing = false;
    private bool lastSpacingState = false;


    public event Action<bool> OnSpacingChanged;

    private void Start()
    {
        if (seperatorRect == null)
            seperatorRect = GetComponent<RectTransform>();

        if (seperatorRect != null)
            originalWidth = seperatorRect.rect.width;
    }

    private void Update()
    {
        if (text != null && logicWordObject != null)
        {
            var wordID = logicWordObject.GetComponent<WordID>();
            if (wordID != null)
            {
                text.text = wordID.word;
                logicWordText = wordID.word;
            }

            var dragAndDrop = logicWordObject.GetComponentInChildren<DragAndDrop>();
            if (dragAndDrop != null)
                isSpacing = dragAndDrop.isSpacing;
        }

        if (isSpacing != lastSpacingState)
        {
            if (isSpacing)
            {
                FMODUnity.RuntimeManager.PlayOneShot("event:/UI/Spacing");
                separator.SetActive(true);
                TweenToWidth(spacing, spacingDuration, spacingEase);
            }
            else
            {
                separator.SetActive(false);
                TweenToWidth(0f, resetDuration, resetEase);
            }

            // 🔑 Fire event
            OnSpacingChanged?.Invoke(isSpacing);

            lastSpacingState = isSpacing;
        }
    }

    private void TweenToWidth(float targetWidth, float duration, Ease ease)
    {
        if (seperatorRect == null) return;

        widthTween?.Kill();
        widthTween = DOTween.To(
                () => seperatorRect.rect.width,
                w => seperatorRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w),
                targetWidth,
                duration
            )
            .SetEase(ease)
            .OnUpdate(ForceRefresh);
    }

    private void LateUpdate()
    {
        if (!target) return;
        FollowTargetRect();
    }

    private void FollowTargetRect()
    {
        var rt = (RectTransform)transform;
        var parentRt = (RectTransform)rt.parent;
        var targetRt = (RectTransform)target;

        var worldCorners = new Vector3[4];
        targetRt.GetWorldCorners(worldCorners);

        var canvas = parentRt.GetComponentInParent<Canvas>();
        var cam = (canvas && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            ? canvas.worldCamera
            : null;

        Vector2 Local(Vector3 w)
        {
            Vector2 sp = RectTransformUtility.WorldToScreenPoint(cam, w);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, sp, cam, out var lp);
            return lp;
        }

        Vector2 bottomLeft = Local(worldCorners[0]);
        Vector2 topRight = Local(worldCorners[2]);

        Vector2 center = (bottomLeft + topRight) * 0.5f;
        Vector2 size = (topRight - bottomLeft);

        rt.anchoredPosition = Vector2.Lerp(
            rt.anchoredPosition,
            center + new Vector2(horizontalOffset, 0f),
            followSpeed * Time.deltaTime
        );

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
    }

    private void ForceRefresh()
    {
        if (seperatorRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(seperatorRect);
    }

    private void OnDisable()
    {
        widthTween?.Kill();
        widthTween = null;
    }
}

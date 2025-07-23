using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(ScrollRect))]
public class ScrollParallaxUI : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        [Tooltip("The RectTransform *inside* your layout wrapper")]
        public RectTransform imageRt;
        [Range(0,1), Tooltip("0 = no movement, 1 = moves exactly with scroll")]
        public float speed = 0.5f;

        [HideInInspector] public Vector2 originalAnchoredPos;
    }

    public ScrollRect scrollRect;            // assign your ScrollRect
    public RectTransform viewport;           // usually scrollRect.viewport
    public List<Layer> layers = new List<Layer>();

    void Start()
    {
        // cache each layer's start position
        foreach (var L in layers)
            L.originalAnchoredPos = L.imageRt.anchoredPosition;

        // hook the scroll event
        scrollRect.onValueChanged.AddListener(OnScroll);
    }

    void OnScroll(Vector2 normPos)
    {
        // center of the viewport in local space is (0,0)
        // normPos.y goes 0→1 as you scroll up/down
        // map normPos.y from [0,1] to [-0.5, +0.5]:
        float t = normPos.y - 0.5f;

        // viewport height in pixels:
        float viewH = viewport.rect.height;

        foreach (var L in layers)
        {
            // how far (in px) we want to nudge this image:
            float offsetY = t * viewH * L.speed;
            L.imageRt.anchoredPosition = L.originalAnchoredPos + Vector2.up * offsetY;
        }
        Debug.Log($"Scroll: {normPos}");
    }
}
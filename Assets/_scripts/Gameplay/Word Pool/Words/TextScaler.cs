using System;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class TextScaler : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private RectTransform rectTransform;

    private float preferredWidth;
    private float preferredHeight;

    [Header("Default Padding")]
    private int horizontalPadding = 28;
    private int verticalPadding = 7;

    [Header("Visual Mode Padding")]
    [SerializeField] private bool isVisual = false;
    private int visualHorizontalPadding = 24; // 👈 this one used if isVisual = true

    private void Update()
    {
        // Calculate text preferred size
        CalculatePreferredSize();

        // Scale rect transform based on text size
        ScaleWithPreferredSize();
    }

    private void ScaleWithPreferredSize()
    {
        if (rectTransform == null && text != null) 
            rectTransform = text.GetComponent<RectTransform>();

        if (rectTransform == null) return;

        Vector2 size = rectTransform.sizeDelta;

        // Pick which padding to use
        int usedHorizontalPadding = isVisual ? visualHorizontalPadding : horizontalPadding;
        int usedVerticalPadding = verticalPadding; // you can also make a visualVerticalPadding if you want

        // Apply padding to preferred size
        size.x = Mathf.CeilToInt(preferredWidth) + usedHorizontalPadding;
        size.y = Mathf.CeilToInt(preferredHeight) + usedVerticalPadding;

        rectTransform.sizeDelta = size;
    }

    private void CalculatePreferredSize()
    {
        if (text == null) return;
        preferredWidth = text.preferredWidth;
        preferredHeight = text.preferredHeight;
    }
}
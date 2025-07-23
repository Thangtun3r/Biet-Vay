using System;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class TextScaler : MonoBehaviour
{
    public TMP_Text text;
    public RectTransform rectTransform;

    private float preferredWidth;
    private float preferredHeight;

    private int horizontalPadding = 20;
    private int verticalPadding = 7;

    private void Update()
    {
        // Calculate text preferred size
        CalculatePreferredSize();

        // Scale rect transform based on text size
        ScaleWithPreferredSize();
    }

    private void ScaleWithPreferredSize()
    {
        if (rectTransform == null) 
            rectTransform = text.GetComponent<RectTransform>();

        Vector2 size = rectTransform.sizeDelta;

        // ✅ Add horizontal padding
        size.x = Mathf.CeilToInt(preferredWidth) + horizontalPadding;

        // ✅ Add vertical padding
        size.y = Mathf.CeilToInt(preferredHeight) + verticalPadding;

        rectTransform.sizeDelta = size;
    }

    private void CalculatePreferredSize()
    {
        preferredWidth = text.preferredWidth;
        preferredHeight = text.preferredHeight;
    }
}
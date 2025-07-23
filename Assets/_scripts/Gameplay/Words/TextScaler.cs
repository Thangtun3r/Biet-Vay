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

    [SerializeField] private int horizontalPadding = 10;
    [SerializeField] private int verticalPadding = 5;

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
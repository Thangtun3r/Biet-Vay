using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class TextScaler : MonoBehaviour
{
    public TMP_Text text;
    public RectTransform rectTransform;
    private float preferredWidth;
    [SerializeField] private int textPadding;

    /*private void Start()
     {
        float preferredWidth = text.preferredWidth;
        Debug.Log(preferredWidth);
    }*/
    
    private void Update()
    {
        //if (Application.isPlaying) return;
        CalculatePreferredWidth();
        ScaleWithWidth();
    }

    // Scale the text width to match the preferred width
    private void ScaleWithWidth()
    {
        if (rectTransform == null) rectTransform = text.GetComponent<RectTransform>();
        Vector2 size = rectTransform.sizeDelta;
        size.x = (int)text.preferredWidth + textPadding;
        rectTransform.sizeDelta = size;
    }

    // Yeh this is just a helper function to calculate the preferred width of the text
    private void CalculatePreferredWidth()
    {
        preferredWidth = text.preferredWidth;
    }
}
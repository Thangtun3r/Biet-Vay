using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class TextScaler : MonoBehaviour
{
    public TMP_Text text;
    public RectTransform rt;
    private float preferredWidth;
    [SerializeField] private float textPadding;

    /*private void Start(){
        text.ForceMeshUpdate();
        
        float preferredWidth = text.preferredWidth;
        Debug.Log(preferredWidth);
    }*/


    private void Update()
    {
        if (Application.isPlaying) return;
        CalculatePreferredWidth();
        ScaleWithWidth();
    }

    private void ScaleWithWidth()
    {
        if (rt == null) rt = text.GetComponent<RectTransform>();
        Vector2 size = rt.sizeDelta;
        size.x = text.preferredWidth + textPadding;
        rt.sizeDelta = size;
    }

    private void CalculatePreferredWidth()
    {
        preferredWidth = text.preferredWidth;
    }
}

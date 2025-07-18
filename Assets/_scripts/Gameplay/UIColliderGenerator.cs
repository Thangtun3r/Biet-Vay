using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(BoxCollider2D))]
public class UIColliderGenerator : MonoBehaviour
{
    public BoxCollider2D BoxCollider;
    public RectTransform rectTransform;
    
    private void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        BoxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        //if (Application.isPlaying) return;
        Generate();
    }

    //Generate a Box collider that matches the size of the RectTransform cause the we have dynamic UI elements
    public void Generate()
    {
        var size = rectTransform.rect.size;
        BoxCollider.size = size;
        BoxCollider.offset = rectTransform.rect.center;
    }
}

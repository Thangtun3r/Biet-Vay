using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReOrdering : MonoBehaviour
{
    public float spacing = 140f; // spacing between words

    private RectTransform rt;
    private Vector2 originalSizeDelta;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (rt != null)
            originalSizeDelta = rt.sizeDelta;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Detector"))
        {
            SpacingOut();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Detector"))
        {
            ResetToOriginalSize();
        }
    }

    private void SpacingOut()
    {
        if (rt != null)
        {
            Vector2 size = rt.sizeDelta;
            size.x = spacing;
            rt.sizeDelta = size;
        }
    }

    public void ResetToOriginalSize()
    {
        if (rt != null)
        {
            rt.sizeDelta = originalSizeDelta;
        }
    }
}
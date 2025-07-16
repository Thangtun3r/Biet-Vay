using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReOrdering : MonoBehaviour
{
    public float spacing = 12f; // spacing between words

    private LayoutElement layoutElement;
    private float originalPreferredWidth;

    private void Awake()
    {
        layoutElement = GetComponent<LayoutElement>();
    }

    private void Start()
    {
        if (layoutElement != null)
            originalPreferredWidth = layoutElement.preferredWidth;
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
        if (layoutElement != null)
        {
            layoutElement.preferredWidth = originalPreferredWidth + spacing;
        }
    }

    public void ResetToOriginalSize()
    {
        if (layoutElement != null)
        {
            layoutElement.preferredWidth = originalPreferredWidth;
        }
    }
}
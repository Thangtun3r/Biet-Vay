using System;
using UnityEngine;
using UnityEngine.UI;

public class DragAndDrop : MonoBehaviour
{
    public float spacing = 12f;
    [SerializeField] private HorizontalLayoutGroup layoutElement;
    
    public bool isSpacing = false;

    private int originalPreferredWidth;

    private void Start()
    {
        if (layoutElement != null)
            originalPreferredWidth = layoutElement.padding.left;
    }

    private void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Detector"))
        {
            isSpacing = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Detector"))
        {
            isSpacing = false;
        }
    }
    
    
}
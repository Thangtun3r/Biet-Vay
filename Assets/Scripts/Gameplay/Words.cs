using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Words : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform parentAfterDrag;
    private Transform overlappedWordTransform;
    public Collider2D detectorCollider;
    [SerializeField] private Collider2D buttonCollider;
    public Detector detectorScript;
    
    private void OnEnable()
    {
        if (detectorScript != null)
            detectorScript.OnWordDetected += HandleWordDetected;
    }

    private void OnDisable()
    {
        if (detectorScript != null)
            detectorScript.OnWordDetected -= HandleWordDetected;
    }

    
    private void HandleWordDetected(Transform detectedObject)
    {
        overlappedWordTransform = detectedObject.transform;
        /*OrderInPosition(true, other.transform);*/
    }

    public void OnPointerDown(PointerEventData eventData)
    {

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        if (detectorCollider != null)
        {
            detectorCollider.enabled = true;
            buttonCollider.enabled = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);
        if (detectorCollider != null)
        {
            detectorCollider.enabled = false;
            buttonCollider.enabled = true;
        }
        
        if (overlappedWordTransform != null)
        {
            int targetIndex = overlappedWordTransform.GetSiblingIndex();
            this.transform.SetSiblingIndex(targetIndex);
            overlappedWordTransform = null;
        }
    }
    
}


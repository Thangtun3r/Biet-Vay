using System;
        using UnityEngine;
        using UnityEngine.EventSystems;
        
        public class Words : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
        {
            Transform parentAfterDrag;
            private Collider2D _collider2D;
            public Collider2D detectorCollider;
        
            private void Awake()
            {
                _collider2D = GetComponent<Collider2D>();
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
                    _collider2D.enabled = false; 
                    detectorCollider.enabled = true;
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
                    _collider2D.enabled = true; 
                    detectorCollider.enabled = false;
                }
                
            }
        }
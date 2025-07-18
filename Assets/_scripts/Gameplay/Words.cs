using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay
{
    public class Words : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Transform _overlappedWordTransform;   // pool or word currently hovered
        private Transform _originalParent;

        private bool _isDragging;             // distinguish click vs drag

        public Collider2D detectorCollider;
        [SerializeField] private Collider2D buttonCollider;
        public Detector detectorScript;

        private void Start()
        {
            _originalParent = transform.parent; 
        }

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
            if (detectedObject.CompareTag("Word") || detectedObject.CompareTag("WordPool"))
            {
                _overlappedWordTransform = detectedObject;
            }
        }
        

        public void OnPointerDown(PointerEventData eventData)
        {
            _isDragging = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                transform.SetParent(_originalParent);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true; 
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
            _isDragging = false; // drag finished
            Transform targetParent = null;

            if (_overlappedWordTransform != null)
            {
                // --- CASE 1: Hovered over another WORD ---
                if (_overlappedWordTransform.CompareTag("Word"))
                {
                    // Get the word's pool (its parent)
                    targetParent = _overlappedWordTransform.parent;

                    // Move into that pool + reorder relative to hovered word
                    transform.SetParent(targetParent);
                    int targetIndex = _overlappedWordTransform.GetSiblingIndex();
                    transform.SetSiblingIndex(targetIndex);
                }
                // --- CASE 2: Hovered over a POOL ---
                else if (_overlappedWordTransform.CompareTag("WordPool"))
                {
                    targetParent = _overlappedWordTransform;

                    // If same as original → snap back
                    if (targetParent == _originalParent)
                    {
                        transform.SetParent(_originalParent);
                    }
                    else
                    {
                        // Different pool → move into it at the end
                        transform.SetParent(targetParent);
                        transform.SetAsLastSibling();
                    }
                }
            }

            // --- CASE 3: Hovered nothing → snap back ---
            if (targetParent == null)
            {
                transform.SetParent(_originalParent);
            }

            // Re-enable colliders for future interaction
            if (detectorCollider != null)
            {
                detectorCollider.enabled = false;
                buttonCollider.enabled = true;
            }

            // reset hover tracking
            _overlappedWordTransform = null;
        }
    }
}

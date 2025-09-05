using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

namespace Gameplay
{
    //remember to revise this script as the function to jump to the opposite pool has changed
    //and may cause significant performance spikes
    public class Words : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
                          IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
   

        [Header("Pool Tag")]
        [SerializeField] private string poolTag = "WordPool"; 

        private Transform _overlappedWordTransform;  
        private Transform _originalParent;

        private bool _isDragging;           

        public Collider2D detectorCollider;
        [SerializeField] private Collider2D buttonCollider;
        public Detector detectorScript;
        
        
        //I use non-static events here cuz i don't want other words to respond to the events
        public event Action<PointerEventData> onPointerDown;
        public event Action<PointerEventData> PointerUpped;
        public event Action<PointerEventData> BeganDrag;
        public event Action<PointerEventData> Dragged;
        public event Action<PointerEventData> EndedDrag;
        
        public event Action<PointerEventData> PointerEntered;
        public event Action<PointerEventData> PointerExited;
        
        //end of event declarations
        

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
            if (detectedObject.CompareTag("Word") || detectedObject.CompareTag(poolTag)) 
            {
                _overlappedWordTransform = detectedObject;
            }
        }
        


        // ======= Unity Pointers Interaction for us to assign ========================================================================================================

        
        public void OnPointerDown(PointerEventData eventdata)
        {
            _isDragging = false;
            onPointerDown?.Invoke(eventdata);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            HandleReleaseBackToPool();       
            PointerUpped?.Invoke(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            BeganDrag?.Invoke(eventData);
            _isDragging = true;
            HandlePullOutOfPool();
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = Input.mousePosition;
            Dragged?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false; 
            HandlePooolLocation();
            EndedDrag?.Invoke(eventData);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerEntered?.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PointerExited?.Invoke(eventData);
        }

        

        // ======= Methods to handle inteactions ========================================================================================================

        private void HandlePullOutOfPool()
        {
            var currentPool = GetCurrentPool(); 
            if (currentPool != null)           
                _originalParent = currentPool;  
            transform.SetParent(transform.root);
            transform.SetAsLastSibling();

            if (detectorCollider != null)
            {
                detectorCollider.enabled = true;
                if (buttonCollider != null) 
                    buttonCollider.enabled = false;
            }
        }
        
        private void HandleReleaseBackToPool()
        {
            if (!_isDragging)
            {
                var currentPool = GetCurrentPool();
                if (currentPool != null)           
                    JumpToOppositePoolByTag(currentPool); 
            }
        }

        private void HandlePooolLocation()
        {
            Transform targetParent = null;
            if (_overlappedWordTransform != null)
            {
                // CASE 1: Hovered over another WORD (same as before)
                if (_overlappedWordTransform.CompareTag("Word"))
                {
                    // Get the word's pool (its parent)
                    targetParent = _overlappedWordTransform.parent;

                    // Move into that pool + reorder relative to hovered word
                    transform.SetParent(targetParent);
                    int targetIndex = _overlappedWordTransform.GetSiblingIndex();
                    transform.SetSiblingIndex(targetIndex);
                }
                // CASE 2: Hovered over a POOL (by tag)
                else if (_overlappedWordTransform.CompareTag(poolTag)) // CHANGED
                {
                    targetParent = _overlappedWordTransform;

                    transform.SetParent(targetParent);
                    transform.SetAsLastSibling();
                }
            }

            // CASE 3: Hovered nothing → snap back to where the drag began
            if (targetParent == null)
            {
                transform.SetParent(_originalParent);
            }
            else
            {
                // NEW: update home after a valid drop
                _originalParent = targetParent; // NEW
            }
            if (detectorCollider != null)
            {
                detectorCollider.enabled = false;
                if (buttonCollider != null)
                    buttonCollider.enabled = true;
            }
            
            _overlappedWordTransform = null;
        }

        private Transform GetCurrentPool() 
        {
            var p = transform.parent;
            return (p != null && p.CompareTag(poolTag)) ? p : null; 
        }
        
        private Transform GetOppositePoolByTag(Transform currentPool) 
        {
            var pools = GameObject.FindGameObjectsWithTag(poolTag)
                                  .Select(go => go.transform)
                                  .Where(t => t != null)
                                  .ToList();

            if (pools.Count == 0) return null;
            if (pools.Count == 1) return pools[0] == currentPool ? null : pools[0];

            // If exactly two: return the other
            if (pools.Count == 2)
                return pools[0] == currentPool ? pools[1] :
                       pools[1] == currentPool ? pools[0] : pools[0];

            // More than two: round-robin by sibling index / scene order
            pools.Sort((a, b) => a.GetSiblingIndex().CompareTo(b.GetSiblingIndex()));
            int idx = pools.IndexOf(currentPool);
            if (idx < 0) idx = 0;
            int next = (idx + 1) % pools.Count;
            return pools[next];
        }

        private void JumpToOppositePoolByTag(Transform currentPool) 
        {
            var opposite = GetOppositePoolByTag(currentPool); 
            if (opposite == null) return;

            transform.SetParent(opposite);       
            transform.SetAsLastSibling();        
            _originalParent = opposite;          
        }


    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

namespace Gameplay
{
    //remember to revise this script as the function to jump to the opposite pool has changed
    //and may cause significant performance spikes
    public class Words : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
                          IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        // OLD: [SerializeField] private Transform poolA;
        // OLD: [SerializeField] private Transform poolB;

        [Header("Pool Tag")]
        [SerializeField] private string poolTag = "WordPool"; // NEW

        private Transform _overlappedWordTransform;   // pool or word currently hovered
        private Transform _originalParent;

        private bool _isDragging;             // distinguish click vs drag

        public Collider2D detectorCollider;
        [SerializeField] private Collider2D buttonCollider;
        public Detector detectorScript;

        private void Start()
        {
            _originalParent = transform.parent;

            // OLD auto-find for 2 pools removed.
            // NEW: nothing to assign; we’ll discover by tag when needed.
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
            // CHANGED: use poolTag instead of hardcoded "WordPool"
            if (detectedObject.CompareTag("Word") || detectedObject.CompareTag(poolTag)) // CHANGED
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
            // CHANGED: click → send to the "opposite" pool discovered by tag
            // OLD:
            // if (!_isDragging)
            // {
            //     transform.SetParent(_originalParent);
            // }

            if (!_isDragging) // NEW
            {                 
                Transform currentPool = GetCurrentPool(); // NEW
                if (currentPool != null)                  
                {                                         
                    JumpToOppositePoolByTag(currentPool); // NEW
                    return;                               
                }                                         
                transform.SetParent(_originalParent);     // fallback (unchanged)
            }                                             
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;

            // NEW: remember the pool we started from for snap-back
            var currentPool = GetCurrentPool(); // NEW
            if (currentPool != null)            // NEW
                _originalParent = currentPool;  // NEW

            transform.SetParent(transform.root);
            transform.SetAsLastSibling();

            if (detectorCollider != null)
            {
                detectorCollider.enabled = true;
                if (buttonCollider != null) // NEW (null-guard)
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

            // Re-enable colliders
            if (detectorCollider != null)
            {
                detectorCollider.enabled = false;
                if (buttonCollider != null) // NEW (null-guard)
                    buttonCollider.enabled = true;
            }

            // reset hover tracking
            _overlappedWordTransform = null;
        }

        // --- Helpers ---

        private Transform GetCurrentPool() // NEW
        {
            var p = transform.parent;
            return (p != null && p.CompareTag(poolTag)) ? p : null; // NEW
        }

        // NEW: if exactly two pools share the tag, pick the other one;
        // if more than two, "opposite" becomes the next pool in a stable round-robin by scene order.
        private Transform GetOppositePoolByTag(Transform currentPool) // NEW
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

        private void JumpToOppositePoolByTag(Transform currentPool) // NEW
        {
            var opposite = GetOppositePoolByTag(currentPool); // NEW
            if (opposite == null) return;

            transform.SetParent(opposite);        // NEW
            transform.SetAsLastSibling();         // NEW
            _originalParent = opposite;           // NEW
        }
    }
}

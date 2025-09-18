using Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

public class FrictionWord : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    private int frictionCount = 0;
    public bool isFriction = false;

    // Reference to the Words component (to disable/enable)
    public Words _words;

    // Reference to the friction follow component
    private FrictionMouseFollow frictionMouseFollow;

    // The visual word object that will be set by VisualToLogic
    private GameObject visualWordObject;

    private void Awake()
    {
        // Get the Words component
        _words = GetComponent<Words>();
    }

    private void OnEnable()
    {
        // Subscribe to the VisualToLogic event to get the visual word object
        var visualToLogic = GetComponent<VisualToLogic>();
        if (visualToLogic != null)
        {
            visualToLogic.OnVisualWordSet += HandleVisualWordSet;
        }
        
        // Initialize FrictionMouseFollow reference if not already set
        if (visualWordObject != null)
        {
            frictionMouseFollow = visualWordObject.GetComponent<FrictionMouseFollow>();
        }
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        var visualToLogic = FindObjectOfType<VisualToLogic>();
        if (visualToLogic != null)
        {
            visualToLogic.OnVisualWordSet -= HandleVisualWordSet;
        }
    }

    // This method is triggered when the OnVisualWordSet event is invoked
    private void HandleVisualWordSet(GameObject visualWord)
    {
        visualWordObject = visualWord;

        // Attach the FrictionMouseFollow component from the visualWordObject
        if (visualWordObject != null)
        {
            frictionMouseFollow = visualWordObject.GetComponent<FrictionMouseFollow>();
        }
    }

    // Pointer Enter (Mouse enters the object)
    public void OnPointerEnter(PointerEventData eventData) { }

    // Pointer Exit (Mouse exits the object)
    public void OnPointerExit(PointerEventData eventData) { }

    // Pointer Down (When mouse button is pressed)
    public void OnPointerDown(PointerEventData eventData)
    {
        // Ensure FrictionMouseFollow is ready for the first time
        if (visualWordObject != null && frictionMouseFollow == null)
        {
            frictionMouseFollow = visualWordObject.GetComponent<FrictionMouseFollow>();
        }

        // Increment friction count when the player starts dragging
        IncrementFrictionCount();

        // Disable the Words script only when the player holds and drags
        if (isFriction && _words != null)
        {
            _words.enabled = false;
        }

        // Start following the mouse when dragging starts
        if (isFriction && frictionMouseFollow != null)
        {
            frictionMouseFollow.canFollowMouse = true;
        }
    }

    // Pointer Up (When mouse button is released)
    public void OnPointerUp(PointerEventData eventData)
    {
        // Re-enable the Words script when the drag stops (on mouse up)
        if (_words != null)
        {
            _words.enabled = true;
        }

        // Stop following the mouse when mouse button is released
        if (isFriction && frictionMouseFollow != null)
        {
            frictionMouseFollow.canFollowMouse = false;
        }
    }

    private void Update()
    {
        // If friction count exceeds 2, re-enable Words and set isFriction to false
        if (frictionCount > 5 && _words != null)
        {
            _words.enabled = true; // Re-enable Words script
            isFriction = false;    // Stop friction once the Words script is re-enabled
            
            // Stop following the mouse by disabling canFollowMouse
            if (frictionMouseFollow != null)
            {
                frictionMouseFollow.StopFollowMouse(); // Disable mouse follow
            }

            frictionCount = 0;     // Reset friction count
        }
    }

    // This method is called to increment the friction count
    public void IncrementFrictionCount()
    {
        frictionCount++; // Increment the friction count each time the player holds and drags
    }
}

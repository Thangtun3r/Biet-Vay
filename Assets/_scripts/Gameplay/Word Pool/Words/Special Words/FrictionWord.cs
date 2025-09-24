using System;
using Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

public class FrictionWord : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private int frictionCount = 0;
    public int maxFrictionCount = 0;
    public bool isFriction = false;
    

    public Words _words;

    [SerializeField] private float followSpeed = 12f;

    private FrictionMouseFollow frictionMouseFollow;
    private GameObject visualWordObject;
    private RectTransform rectTransform;

    private bool isDragging; // tracks drag state

    private void Awake()
    {
        _words = GetComponent<Words>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        var visualToLogic = GetComponent<VisualToLogic>();
        if (visualToLogic != null)
            visualToLogic.OnVisualWordSet += HandleVisualWordSet;
        
         
            
    }
    

    private void OnDisable()
    {
        var visualToLogic = FindObjectOfType<VisualToLogic>();
        if (visualToLogic != null)
            visualToLogic.OnVisualWordSet -= HandleVisualWordSet;
    }

    private void HandleVisualWordSet(GameObject visualWord)
    {
        visualWordObject = visualWord;
        if (visualWordObject != null)
            frictionMouseFollow = visualWordObject.GetComponent<FrictionMouseFollow>();
        if (isFriction)
        {
            frictionMouseFollow.isFriction();
            _words.enabled = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) { }
    public void OnPointerExit(PointerEventData eventData) { }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isFriction && _words != null)
            _words.enabled = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_words != null) _words.enabled = true;
        if (isFriction && frictionMouseFollow != null) frictionMouseFollow.StopFollowMouse();
    }

    // 🔹 Click handling only here
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging) return; // block click after drag
        if (isFriction && _words != null)
        {
            frictionMouseFollow?.ClickShake();
        }
       
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        eventData.eligibleForClick = false; // prevent click firing at end of drag

        IncrementFrictionCount();

        if (isFriction && _words != null) _words.enabled = false;
        if (isFriction && frictionMouseFollow != null) frictionMouseFollow.FollowMouse();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isFriction && _words != null) _words.enabled = false;
        if (isFriction && frictionMouseFollow != null) frictionMouseFollow.FollowMouse();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        if (_words != null) _words.enabled = true;
        if (frictionMouseFollow != null) frictionMouseFollow.StopFollowMouse();
    }

    private void Update()
    {
        if (frictionCount > maxFrictionCount && _words != null)
        {
            _words.enabled = true;
            isFriction = false;
            if (frictionMouseFollow != null)
                frictionMouseFollow.StopFollowMouse();
        }
    }

    public void IncrementFrictionCount() => frictionCount++;
}

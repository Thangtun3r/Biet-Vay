using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay;
using UnityEngine;
using FMODUnity;   // FMOD
using FMOD.Studio;
using UnityEngine.EventSystems;

public class WordSFX : MonoBehaviour
{
    [Header("FMOD")]
    [Tooltip("Prefix for FMOD event paths, e.g. 'event:/UI/'")]
    [SerializeField] private string eventPrefix = "event:/UI/";

    [Header("SFX")]
    [Tooltip("Event name appended to the prefix, e.g. 'UI_Hover'")]
    [SerializeField] private string hoverSFX = "UI_Hover";
    [SerializeField] private string dragSFX = "Drag";
    

    private Words words;
    public static bool isDragging;
    private float lastHoverTime = -999f;

    public static event Action OnDraggingSFX;

    private void Awake()
    {
        words = GetComponent<Words>();
    }

    private void OnEnable()
    {
        if (words == null) return;
        
        words.BeganDrag += HandleBeganDrag;
        words.EndedDrag += HandleEndedDrag;
        words.PointerEntered += HandlePointerEntered;   // <-- hover only
        words.PointerExited += HandlePointerExited;     // (optional, not used yet)
        words.PointerUpped += HandlePointerUpped;       // (optional, not used yet)
    }

    private void OnDisable()
    {
        if (words == null) return;

        words.BeganDrag -= HandleBeganDrag;
        words.EndedDrag -= HandleEndedDrag;
        words.PointerEntered -= HandlePointerEntered;
        words.PointerExited -= HandlePointerExited;
        words.PointerUpped -= HandlePointerUpped;

        isDragging = false;
    }

    // --- Hover only ---
    private void HandlePointerEntered(PointerEventData _)
    {
        if (isDragging) return; // don't hover while dragging
        

        PlayOneShot(hoverSFX);
        lastHoverTime = Time.unscaledTime;
    }

    private void HandleBeganDrag(PointerEventData _)
    {
        isDragging = true;
        PlayOneShot(dragSFX);
        
    }
    private void HandleEndedDrag(PointerEventData _)  { isDragging = false; }

    // Not used yet; kept for future expansion
    private void HandlePointerExited(PointerEventData _) { }
    private void HandlePointerUpped(PointerEventData _ ){ }

    // --- FMOD helper ---
    private void PlayOneShot(string sfxName)
    {
        if (string.IsNullOrEmpty(sfxName)) return;
        string path = string.Concat(eventPrefix, sfxName);
        RuntimeManager.PlayOneShot(path, transform.position);
    }
}

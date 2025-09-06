using System;
using Gameplay;
using UnityEngine;
using FMODUnity;
using UnityEngine.EventSystems;

public class WordSFX : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private DragAndDrop _dragAndDrop;

    [Header("FMOD")]
    [SerializeField] private string eventPrefix = "event:/UI/";

    [Header("SFX")]
    [SerializeField] private string hoverSFX   = "UI_Hover";
    [SerializeField] private string dragSFX    = "Drag";
    [SerializeField] private string clickSFX   = "Click";
    [SerializeField] private string spacingSFX = "Spacing";

    [Header("Behavior")]
    [Tooltip("Global cooldown shared by ALL WordSFX (seconds, unscaled).")]
    [SerializeField] private float globalHoverCooldown = 0.25f;

    // ---- GLOBAL GATE ----
    private static float s_LastHoverPlayTime = -999f;

    private Words words;
    public static bool isDragging;
    private bool pressedWithoutDrag;

    private void Awake()
    {
        words = GetComponent<Words>();
        if (_dragAndDrop == null) _dragAndDrop = GetComponentInChildren<DragAndDrop>(true);
    }

    private void OnEnable()
    {
        if (words == null) return;
        words.BeganDrag      += HandleBeganDrag;
        words.EndedDrag      += HandleEndedDrag;
        words.PointerEntered += HandlePointerEntered;
        words.PointerExited  += HandlePointerExited;
        words.PointerUpped   += HandlePointerUpped;
        words.onPointerDown  += HandlePointerDown;
    }

    private void OnDisable()
    {
        if (words == null) return;
        words.BeganDrag      -= HandleBeganDrag;
        words.EndedDrag      -= HandleEndedDrag;
        words.PointerEntered -= HandlePointerEntered;
        words.PointerExited  -= HandlePointerExited;
        words.PointerUpped   -= HandlePointerUpped;
        words.onPointerDown  -= HandlePointerDown;

        isDragging = false;
        pressedWithoutDrag = false;
    }

    // --- Hover ---
    private void HandlePointerEntered(PointerEventData _)
    {
        if (isDragging) return;

        float now = Time.unscaledTime;

        // GLOBAL throttle across all WordSFX
        if (now - s_LastHoverPlayTime < globalHoverCooldown) return;

        PlayOneShot(hoverSFX);
        s_LastHoverPlayTime = now;
    }

    private void HandlePointerExited(PointerEventData _) { }

    // --- Click / Drag gating ---
    private void HandlePointerDown(PointerEventData _)  => pressedWithoutDrag = true;

    private void HandleBeganDrag(PointerEventData _)
    {
        isDragging = true;
        pressedWithoutDrag = false;
        PlayOneShot(dragSFX);
    }

    private void HandleEndedDrag(PointerEventData _) => isDragging = false;

    private void HandlePointerUpped(PointerEventData _)
    {
        if (pressedWithoutDrag && !isDragging) PlayOneShot(clickSFX);
        pressedWithoutDrag = false;
    }

    // --- FMOD helper ---
    private void PlayOneShot(string sfxName)
    {
        if (string.IsNullOrEmpty(sfxName)) return;
        RuntimeManager.PlayOneShot(string.Concat(eventPrefix, sfxName), transform.position);
    }
}

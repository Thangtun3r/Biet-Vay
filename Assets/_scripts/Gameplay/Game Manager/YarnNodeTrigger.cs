using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Yarn.Unity;
using TMPro; // <-- TextMeshPro

public class YarnNodeTrigger : MonoBehaviour, IPlayerInteraction
{
    [SerializeField] private string yarnNodeName;

    [Header("Highlight Settings")]
    [SerializeField] private GameObject highlightTarget; // The object whose layer will change
    private string highlightLayerName = "Fill";

    private int defaultLayer;
    private int highlightLayer;

    // "Pulse" style highlight input: call Highlight() each frame while hovered
    private bool isHighlighting = false;
    private bool wasHighlighted = false; // track last applied state

    [Header("Floating Text (optional)")]
    [Tooltip("Drag the Floating Text object here (must have TMP_Text either on it or in children).")]
    [SerializeField] private GameObject floatingTextObject;
    [Tooltip("What the floating text should show while highlighted.")]
    [SerializeField] private string highlightText = "Interact";

    private TMP_Text floatingTMP;
    private string originalTMPText;
    private bool hasFloatingText = false;

    [Header("Enable Look At for the girl ?")]
    public bool girlLookAt;
    [SerializeField] private GirlLookAt girlLookAtTarget;
    
    [Header("Is this a prop object ?")]
    [Tooltip("If true, it will be disabled after interaction.")]
    public bool isProp;
    public EventReference propSound;

    private void Start()
    {
        // Highlight target + layers
        if (highlightTarget == null)
        {
            Debug.LogWarning($"{name}: No highlightTarget assigned!");
        }
        else
        {
            defaultLayer = highlightTarget.layer;
            highlightLayer = LayerMask.NameToLayer(highlightLayerName);

            if (highlightLayer == -1)
            {
                Debug.LogError($"Layer \"{highlightLayerName}\" not found! Please add it in Project Settings > Tags and Layers.");
            }
        }

        // Floating text hookup
        if (floatingTextObject != null)
        {
            floatingTMP = floatingTextObject.GetComponentInChildren<TMP_Text>(true);
            if (floatingTMP != null)
            {
                originalTMPText = floatingTMP.text;
                hasFloatingText = true;
            }
            else
            {
                Debug.LogWarning($"{name}: Floating Text object has no TMP_Text in self or children.");
            }
        }
    }

    private void Update()
    {
        // consume the one-frame pulse
        bool wantsHighlight = isHighlighting;
        isHighlighting = false;

        // Only apply changes when the state flips
        if (wantsHighlight != wasHighlighted)
        {
            ApplyHighlightState(wantsHighlight);
            wasHighlighted = wantsHighlight;
        }
    }

    public void Interact()
    {
        YarnDialogueEventBridge.CallYarnEvent(yarnNodeName);
        HandleGirlLookAt();
        HandleDisableProp();
    }

    private void HandleGirlLookAt()
    {
        if (girlLookAt && girlLookAtTarget != null)
        {
            girlLookAtTarget.YawLookAtTarget();
        }
    }

    /// <summary>
    /// Call this each frame while the player is focusing/hovering this object.
    /// </summary>
    public void Highlight()
    {
        isHighlighting = true;
    }

    /// <summary>
    /// Apply both layer swap and floating text change based on highlight state.
    /// </summary>
    private void ApplyHighlightState(bool on)
    {
        // Layers
        if (highlightTarget != null)
        {
            int targetLayer = on ? highlightLayer : defaultLayer;
            if (highlightTarget.layer != targetLayer)
            {
                SetLayerRecursively(highlightTarget, targetLayer);
            }
        }

        // Floating text
        SetFloatingTextHighlighted(on);
    }

    private void SetFloatingTextHighlighted(bool on)
    {
        if (!hasFloatingText) return;

        if (on)
        {
            if (!string.IsNullOrEmpty(highlightText) && floatingTMP.text != highlightText)
                floatingTMP.text = highlightText;
        }
        else
        {
            if (floatingTMP.text != originalTMPText)
                floatingTMP.text = originalTMPText;
        }
    }

    private void HandleDisableProp()
    {
        if (isProp)
        {
            RuntimeManager.PlayOneShot(propSound);
            gameObject.SetActive(false);
        }
    }
    
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}

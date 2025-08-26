using System;
using UnityEngine;
using UnityEngine.UI;

public class DragAndDrop : MonoBehaviour
{
    public float spacing = 12f;
    [SerializeField] private HorizontalLayoutGroup layoutElement;

    public bool isSpacing = false;

    private int originalPreferredWidth;

    // GLOBAL GATE: only one instance can be active at a time
    private static bool s_ActiveInDetector = false;

    private void Start()
    {
        if (layoutElement != null)
            originalPreferredWidth = layoutElement.padding.left;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Detector")) return;

        // If nobody owns the gate, claim it and become active.
        if (!s_ActiveInDetector)
        {
            s_ActiveInDetector = true;
            isSpacing = true;
            return;
        }

        // If someone else already owns it, stay inactive.
        // (If we're the current owner, just keep isSpacing true.)
        if (!isSpacing) return;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Detector")) return;

        // Only the instance that owns the gate can release it.
        if (isSpacing)
        {
            isSpacing = false;
            s_ActiveInDetector = false;
        }
    }

    // Safety: if the active one gets disabled/destroyed while owning the gate, release it.
    private void OnDisable()
    {
        if (isSpacing)
        {
            isSpacing = false;
            s_ActiveInDetector = false;
        }
    }
}
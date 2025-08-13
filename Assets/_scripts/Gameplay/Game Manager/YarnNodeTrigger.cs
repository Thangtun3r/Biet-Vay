using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YarnNodeTrigger : MonoBehaviour, IPlayerInteraction
{
    [SerializeField] private string yarnNodeName;
    [SerializeField] private Material highlightMaterial;
    [NonSerialized] private GameObject floatingLabel;

    private bool isHighlighting = false;
    private Material originalMaterial;
    private Renderer renderer;

    private void Start()
    {
        renderer = GetComponent<Renderer>();
        originalMaterial = renderer.material;
    }
    private void Update()
    {
        HandleMats();
    }


    public void Interact()
    {
        YarnDialogueEventBridge.CallYarnEvent(yarnNodeName);
    }

    public void Highlight()
    {
        isHighlighting = true;
    }

    private void HandleMats()
    {
        if (isHighlighting)
        {
            if (renderer.material != highlightMaterial)
                renderer.material = highlightMaterial;
        }
        else
        {
            if (renderer.material != originalMaterial)
                renderer.material = originalMaterial;
        }
        isHighlighting = false;
    }


    // ======= Handle TMP Floating ========================================================================================================

    
    
}

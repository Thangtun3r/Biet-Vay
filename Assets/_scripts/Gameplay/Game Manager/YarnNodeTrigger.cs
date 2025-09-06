using System;
using UnityEngine;
using Yarn.Unity;

public class YarnNodeTrigger : MonoBehaviour, IPlayerInteraction
{
    [SerializeField] private string yarnNodeName;

    [Header("Highlight Settings")]
    [SerializeField] private GameObject highlightTarget; // The object whose layer will change
    [SerializeField] private string highlightLayerName = "Fill"; // Target layer name

    private int defaultLayer;
    private int highlightLayer;
    private bool isHighlighting = false;

    [Header("Enable Look At for the girl ?")]
    public bool girlLookAt;
    [SerializeField] private GirlLookAt girlLookAtTarget;

    private void Start()
    {
        if (highlightTarget == null)
        {
            Debug.LogWarning($"{name}: No highlightTarget assigned!");
            return;
        }

        defaultLayer = highlightTarget.layer;
        highlightLayer = LayerMask.NameToLayer(highlightLayerName);

        if (highlightLayer == -1)
        {
            Debug.LogError($"Layer \"{highlightLayerName}\" not found! Please add it in Project Settings > Tags and Layers.");
        }
    }

    private void Update()
    {
        HandleLayer();
    }

    public void Interact()
    {
        YarnDialogueEventBridge.CallYarnEvent(yarnNodeName);
        HandleGirlLookAt();
    }

    private void HandleGirlLookAt()
    {
        if (girlLookAt && girlLookAtTarget != null)
        {
            girlLookAtTarget.YawLookAtTarget();
        }
    }

    public void Highlight()
    {
        isHighlighting = true;
    }

    private void HandleLayer()
    {
        if (highlightTarget == null) return;

        if (isHighlighting)
        {
            if (highlightTarget.layer != highlightLayer)
                highlightTarget.layer = highlightLayer;
        }
        else
        {
            if (highlightTarget.layer != defaultLayer)
                highlightTarget.layer = defaultLayer;
        }

        isHighlighting = false;
    }
}
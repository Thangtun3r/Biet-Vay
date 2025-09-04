using System;
using UnityEngine;

public class BietVayTriggerForSentencePool : MonoBehaviour
{
    private WordMarkup _wordMarkup;

    [Tooltip("Only trigger events for objects on this layer.")]
    public LayerMask detectionLayer;


    private void Awake()
    {
        _wordMarkup = GetComponentInParent<WordMarkup>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!IsInDetectionLayer(other.gameObject)) return;


        _wordMarkup?.RaiseBietVayEvent();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsInDetectionLayer(other.gameObject)) return;

        _wordMarkup?.RaiseReleaseEvent();
    }

    private bool IsInDetectionLayer(GameObject obj)
    {
        return ((1 << obj.layer) & detectionLayer) != 0;
    }
}
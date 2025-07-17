using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    public Collider2D _collider2D;
    public Transform detectedObjectPosition;
    
    public event Action<Transform> OnWordDetected;

    private void Start()
    {
        _collider2D = GetComponent<Collider2D>();
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Word"))
        {
            // If the detected object is a word, invoke the event with its position to the Words.cs
            detectedObjectPosition = other.transform.parent;
            OnWordDetected?.Invoke(detectedObjectPosition);
        }

        if (other.CompareTag("Wordpool"))
        {
            
        }
    }
}

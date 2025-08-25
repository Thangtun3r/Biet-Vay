using System;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]

public class Detector : MonoBehaviour
{
    public Collider2D _collider2D;

    public Transform detectedObjectPosition;  // parent pool of a word
    public Transform detectedPoolPosition;    // direct pool detection

    public event Action<Transform> OnWordDetected;

    private void Start()
    {
        _collider2D = GetComponent<Collider2D>();
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        // Hovering over a Word → get its parent (which should be the pool)
        if (other.CompareTag("Word"))
        {
            // Climb to parent (pool)
            detectedObjectPosition = other.transform.parent; 

            // Pass the pool, not the word itself
            OnWordDetected?.Invoke(detectedObjectPosition);
        }
        // Hovering over a WordPool directly
        else if (other.CompareTag("WordPool"))
        {
            detectedPoolPosition = other.transform;

            // Pass the pool itself
            OnWordDetected?.Invoke(detectedPoolPosition);
        }
    }
}


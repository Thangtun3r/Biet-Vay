using System;
using UnityEngine;
using DG.Tweening;

public class GirlLookAt : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;                    // Object to look at
    public Transform objectToRotate;            // Object that will rotate
    public float yawOffsetDegrees = 0f;         // Yaw offset (positive = right, negative = left)

    [Header("Tween Settings")]
    public float dampTime = 0.5f;               // Damping duration
    public Ease easeType = Ease.OutQuad;        // Tween easing

    private Quaternion originalRotation;        // Stores the original rotation for reset

    private void Start()
    {
        if (objectToRotate != null)
        {
            originalRotation = objectToRotate.rotation;
        }
    }

    private void OnEnable()
    {
        GameManager.OnExpand += ResetRotation;
    }
    private void OnDisable()
    {
        GameManager.OnExpand -= ResetRotation;
    }

    /// <summary>
    /// Rotates the object toward the target with yaw-only and damping.
    /// </summary>
    public void YawLookAtTarget()
    {
        if (target == null || objectToRotate == null) return;

        Vector3 direction = target.position - objectToRotate.position;
        direction.y = 0f;

        if (direction == Vector3.zero) return;

        Quaternion baseRotation = Quaternion.LookRotation(direction);
        Quaternion offsetRotation = Quaternion.Euler(0f, yawOffsetDegrees, 0f);
        Quaternion finalRotation = baseRotation * offsetRotation;

        objectToRotate.DOKill();

        objectToRotate
            .DORotateQuaternion(finalRotation, dampTime)
            .SetEase(easeType);
    }

    /// <summary>
    /// Resets the rotated object back to its original rotation.
    /// </summary>
    public void ResetRotation()
    {
        if (objectToRotate == null) return;

        objectToRotate.DOKill(); // Stop any ongoing tweens

        objectToRotate
            .DORotateQuaternion(originalRotation, dampTime)
            .SetEase(easeType);
    }
}
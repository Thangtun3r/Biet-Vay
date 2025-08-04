using UnityEngine;

public class VisualWord : MonoBehaviour
{
    public Transform target;
    public float followSpeed = 10f;
    public float horizontalOffset = 0f; // Positive for right, negative for left

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate offset position
        Vector3 offsetPosition = target.position + target.right * horizontalOffset;

        // Smooth follow (position only)
        transform.position = Vector3.Lerp(transform.position, offsetPosition, followSpeed * Time.deltaTime);

        // If you also want rotation:
        // transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, followSpeed * Time.deltaTime);
    }
}
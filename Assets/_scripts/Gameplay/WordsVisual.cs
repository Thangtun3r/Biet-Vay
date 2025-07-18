using UnityEngine;
using UnityEngine.EventSystems; // Needed for UI raycast checks if needed

[RequireComponent(typeof(Collider2D))] // Needed for mouse detection
public class DraggableChildFollower : MonoBehaviour
{
    [Header("Follow Settings")]
    public float followSpeed = 10f;      // How smooth it follows parent
    public float rotateSpeed = 10f;      // Smooth rotation speed
    public bool smoothFollow = true;     // Toggle smooth vs instant

    private Transform parentTransform;
    private bool isDragging = false;
    private Vector3 mouseOffset;
    private Camera mainCam;

    void Awake()
    {
        if (transform.parent != null)
            parentTransform = transform.parent;
        else
            Debug.LogWarning($"{gameObject.name} has no parent to follow!");

        mainCam = Camera.main;
    }

    void Update()
    {
        if (isDragging)
        {
            // While dragging, move with the mouse in 2D
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            transform.position = mouseWorldPos + mouseOffset;
        }
        else
        {
            // When not dragging, smoothly follow its parent
            FollowParent();
        }
    }

    private void FollowParent()
    {
        if (parentTransform == null) return;

        if (smoothFollow)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                parentTransform.position,
                followSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                parentTransform.rotation,
                rotateSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.position = parentTransform.position;
            transform.rotation = parentTransform.rotation;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = -mainCam.transform.position.z; // distance from camera
        return mainCam.ScreenToWorldPoint(mouseScreenPos);
    }

    void OnMouseDown()
    {
        // Calculate offset so it doesn't jump when clicked
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        mouseOffset = transform.position - mouseWorldPos;
        isDragging = true;
    }

    void OnMouseUp()
    {
        isDragging = false;
    }
}

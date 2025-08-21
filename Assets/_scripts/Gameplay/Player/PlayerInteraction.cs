using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float raycastDistance = 5f;
    [SerializeField] private RectTransform renderImageRect;
    [SerializeField] private GameObject crosshair; // ✅ Crosshair UI element

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        // Make sure crosshair is off at start
        if (crosshair != null)
            crosshair.SetActive(false);
    }

    private void Update()
    {
        HandleInteraction();
    }

    private void HandleInteraction()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 localPoint;

        bool interactableInSight = false;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(renderImageRect, screenCenter, null, out localPoint))
        {
            Vector2 normalizedPoint = Rect.PointToNormalized(renderImageRect.rect, localPoint);
            Ray ray = mainCamera.ViewportPointToRay(normalizedPoint);

            Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red);

            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
            {
                IPlayerInteraction interactable = hit.collider.GetComponent<IPlayerInteraction>();

                if (interactable != null)
                {
                    interactable.Highlight();
                    interactableInSight = true;

                    if (Input.GetMouseButtonDown(0))
                    {
                        interactable.Interact();
                    }
                }
            }
        }

        // ✅ Toggle crosshair based on interactable presence
        if (crosshair != null)
        {
            crosshair.SetActive(interactableInSight);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float raycastDistance = 5f; // Distance for raycasting to detect interactable objects
    [SerializeField] private RectTransform renderImageRect; // Your RawImage RectTransform
    
    private Camera mainCamera;
    


    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleInteraction();
    }

    private void HandleInteraction()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 localPoint;

        // Convert screen point to local UI rect space
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(renderImageRect, screenCenter, null, out localPoint))
        {
            // Convert to world point in the RenderCamera
            Vector2 normalizedPoint = Rect.PointToNormalized(renderImageRect.rect, localPoint);
            Ray ray = mainCamera.ViewportPointToRay(normalizedPoint);

            Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red);

            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
            {
                IPlayerInteraction interactable = hit.collider.GetComponent<IPlayerInteraction>();

                if (interactable != null)
                {
                    interactable.Highlight();

                    if (Input.GetMouseButtonDown(0))
                    {
                        interactable.Interact();
                    }
                }
            }
        }
    }

}

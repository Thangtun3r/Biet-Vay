using System;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float raycastDistance = 5f;

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
        bool interactableInSight = false;

        // Get the ray from the center of the screen
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

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
}
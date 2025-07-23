using UnityEngine;
using TMPro; // for TMP_Text

public class PlayerRayInteractor : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;                       // The camera from which the ray will shoot
    public float interactDistance = 3f;               // Max ray distance
    public LayerMask interactableLayer;               // Layer for interactables
    public TMP_Text interactIndicator;                // UI text like "Press F to interact"
    public WordPackageRandomizer wordPackageRandomizer; // Reference to the receiver

    private WordPackageProvider currentTarget;        // Current interactable target

    void Update()
    {
        HandleRaycast();
        HandleInput();
    }

    private void HandleRaycast()
    {
        // Default: hide indicator
        if (interactIndicator != null)
            interactIndicator.gameObject.SetActive(false);

        currentTarget = null;

        // Shoot a ray from the center of the screen
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactableLayer))
        {
            WordPackageProvider provider = hit.collider.GetComponent<WordPackageProvider>();

            if (provider != null)
            {
                currentTarget = provider;

                // Show TMP indicator
                if (interactIndicator != null)
                {
                    interactIndicator.gameObject.SetActive(true);
                    interactIndicator.text = "Press <b>F</b> to interact";
                }
            }
        }
    }

    private void HandleInput()
    {
        if (currentTarget != null && Input.GetKeyDown(KeyCode.F))
        {
            WordsPackage package = currentTarget.GetPackage();

            if (package != null && wordPackageRandomizer != null)
            {
                wordPackageRandomizer.LoadAndPour(package);
                Debug.Log($"[PlayerRayInteractor] Sent package: {package.name}");
            }
        }
    }
}

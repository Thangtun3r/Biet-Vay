using UnityEngine;

public class Billboard : MonoBehaviour
{
    public bool keepUpright = true; // If true, keeps the billboard upright (useful for trees, UI, etc.)

    private Transform camTransform;

    void Start()
    {
        // Get the main camera's transform
        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("No Main Camera found. Please tag your camera as 'MainCamera'.");
        }
    }

    void LateUpdate()
    {
        if (camTransform == null) return;

        if (keepUpright)
        {
            // Rotate only around Y-axis to stay upright
            Vector3 lookPos = camTransform.position - transform.position;
            lookPos.y = 0; // Ignore vertical tilt
            transform.rotation = Quaternion.LookRotation(-lookPos);
        }
        else
        {
            // Fully face the camera
            transform.LookAt(camTransform);
            transform.rotation = Quaternion.LookRotation(transform.position - camTransform.position);
        }
    }
}
using UnityEngine;

public class URLRedirector : MonoBehaviour
{
    // You can set this URL directly in the Unity Inspector
    [SerializeField] private string targetURL = "https://www.example.com";

    // This public method can be called from UI Buttons or other scripts
    public void RedirectToURL()
    {
        if (!string.IsNullOrEmpty(targetURL))
        {
            Application.OpenURL(targetURL);
            Debug.Log("Redirecting to: " + targetURL);
        }
        else
        {
            Debug.LogWarning("No URL set! Please assign a target URL in the Inspector.");
        }
    }
}
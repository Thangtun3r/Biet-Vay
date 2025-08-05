using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class IgnoreRaycast : MonoBehaviour
{
    private void Awake()
    {
        // Get any Graphic component (Image, Text, TMP, etc.)
        var graphic = GetComponent<Graphic>();
        
        // Disable raycast target so it won’t block clicks
        graphic.raycastTarget = false;
    }
}
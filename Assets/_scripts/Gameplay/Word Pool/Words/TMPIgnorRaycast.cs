using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TMPIgnoreRaycast : MonoBehaviour
{
    void Awake()
    {
        TMP_Text tmp = GetComponent<TMP_Text>();
        if (tmp != null)
        {
            tmp.raycastTarget = false; // Disable raycast blocking
        }
    }
}
using UnityEngine;

public class PsudoStateManager : MonoBehaviour
{
    [Header("Objects to Toggle")]
    [Tooltip("Assign all objects that should be disabled in 3D mode and enabled in 2D mode.")]
    public GameObject[] objectsToToggle;

    private bool is3DMode = false;

    /// <summary>
    /// Switches the game state to 3D.
    /// Disables all assigned GameObjects.
    /// </summary>
    public void SwitchTo3D()
    {
        is3DMode = true;
        SetObjectsActive(false);
    }

    /// <summary>
    /// Switches the game state to 2D.
    /// Re-enables all assigned GameObjects.
    /// </summary>
    public void SwitchTo2D()
    {
        is3DMode = false;
        SetObjectsActive(true);
    }

    /// <summary>
    /// Helper method to enable/disable all objects.
    /// </summary>
    private void SetObjectsActive(bool active)
    {
        foreach (var obj in objectsToToggle)
        {
            if (obj != null) obj.SetActive(active);
        }
    }

    /// <summary>
    /// Optional toggle method if you want to switch between modes dynamically.
    /// </summary>
    public void ToggleMode()
    {
        if (is3DMode)
            SwitchTo2D();
        else
            SwitchTo3D();
    }
}
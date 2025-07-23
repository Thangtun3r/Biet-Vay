using UnityEngine;

public class PoolResetterButton : MonoBehaviour
{
    [Header("References")]
    public WordPackageRandomizer wordPackageRandomizer; // Assign the same randomizer used for pouring

    /// <summary>
    /// Called from a UI Button's OnClick event to clear the pools
    /// </summary>
    public void ResetPools()
    {
        if (wordPackageRandomizer != null)
        {
            wordPackageRandomizer.ClearPool();
            Debug.Log("[PoolResetterButton] Pools cleared. Ready for next interaction.");
        }
    }
}
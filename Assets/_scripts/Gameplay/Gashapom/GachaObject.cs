using UnityEngine;

public class GachaObject : MonoBehaviour
{
    [Header("Where to spawn the selected prefab")]
    [SerializeField] private Transform spawnRoot;  // leave null = use this.transform

    [Header("Debug / Info")]
    [SerializeField] private string currentName;

    private GameObject currentInstance;

    private void OnEnable()  => GachaMachine.OnGachaRolled += Apply;
    private void OnDisable() => GachaMachine.OnGachaRolled -= Apply;

    public void Apply(GachaObjectSO data)
    {
        if (data == null || data.prefab == null) return;

        // Destroy old
        if (currentInstance != null)
        {
            Destroy(currentInstance);
        }

        // Spawn new as child
        var parent = spawnRoot ? spawnRoot : transform;
        currentInstance = Instantiate(data.prefab, parent);

        // Reset transform under parent
        currentInstance.transform.localPosition = Vector3.zero;
        currentInstance.transform.localRotation = Quaternion.identity;
        currentInstance.transform.localScale    = Vector3.one;

        currentName = data.displayName;
    }
}
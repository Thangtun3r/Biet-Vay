using UnityEngine;

public class LabelSpawner : MonoBehaviour
{
    [Tooltip("The prefab to spawn (should already be a World Space Canvas object with text)")]
    public GameObject labelPrefab;

    [Tooltip("Offset above the object")]
    public Vector3 offset = new Vector3(0, 1f, 0);

    private GameObject spawnedLabel;

    void Start()
    {
        if (labelPrefab == null)
        {
            Debug.LogError("Label prefab not assigned!", this);
            return;
        }

        // Spawn label
        spawnedLabel = Instantiate(labelPrefab, transform.position + offset, Quaternion.identity);

        // Make it follow this object
        spawnedLabel.transform.SetParent(transform, worldPositionStays: true);
    }
}

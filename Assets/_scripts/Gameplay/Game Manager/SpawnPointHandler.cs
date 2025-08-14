using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointHandler : MonoBehaviour
{
    public static event Action<Transform> OnPlayerSpawn;

    [Serializable]
    public class SpawnPoint
    {
        public string name;
        public Transform position;
    }

    [SerializeField] private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    // Case-insensitive + static cache
    private static readonly Dictionary<string, Transform> spawnPointDictionary =
        new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);

    private static bool initialized;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;

        // Fill once from the first active handler in the scene
        foreach (var sp in spawnPoints)
        {
            if (sp?.position == null || string.IsNullOrWhiteSpace(sp.name)) continue;

            var key = sp.name.Trim();
            if (!spawnPointDictionary.ContainsKey(key))
            {
                spawnPointDictionary.Add(key, sp.position);
            }
        }
#if UNITY_EDITOR
        Debug.Log($"[SpawnPointHandler] Registered {spawnPointDictionary.Count} spawn points");
#endif
    }

    public static void SetPlayerSpawnPoint(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Debug.LogWarning("[SpawnPointHandler] Empty spawn name");
            return;
        }

        var key = name.Trim();

        if (spawnPointDictionary.TryGetValue(key, out var spawnPoint))
        {
            OnPlayerSpawn?.Invoke(spawnPoint);
        }
        else
        {
            Debug.LogWarning(
                $"[SpawnPointHandler] Spawn '{key}' not found. " +
                $"Have: {string.Join(", ", spawnPointDictionary.Keys)}");
        }
    }
}
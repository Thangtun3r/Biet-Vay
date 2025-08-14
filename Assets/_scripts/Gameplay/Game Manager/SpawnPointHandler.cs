using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class SpawnPointHandler : MonoBehaviour
{
    [Serializable]
    public struct NamedSpawnPoint
    {
        public string name;
        public Transform point;
    }

    [SerializeField] private List<NamedSpawnPoint> spawnPoints = new();
    private Dictionary<string, Transform> _lookup;

    public static SpawnPointHandler Instance { get; private set; }
    public static event Action<Transform> OnPlayerSpawn;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildLookup();
    }

    private void BuildLookup()
    {
        _lookup = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);
        foreach (var sp in spawnPoints)
        {
            if (!string.IsNullOrWhiteSpace(sp.name) && sp.point != null)
            {
                _lookup[sp.name.Trim()] = sp.point;
            }
        }
    }

    public static bool TryGetSpawnPoint(string name, out Transform point)
    {
        point = null;
        return Instance != null &&
               Instance._lookup.TryGetValue((name ?? string.Empty).Trim(), out point);
    }

    public static void InvokePlayerSpawn(Transform spawnPoint)
    {
        if (spawnPoint != null)
            OnPlayerSpawn?.Invoke(spawnPoint);
    }
}
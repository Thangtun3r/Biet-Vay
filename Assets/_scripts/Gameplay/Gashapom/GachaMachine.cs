using System;
using System.Collections.Generic;
using UnityEngine;

public class GachaMachine : MonoBehaviour
{
    public static event Action<GachaObjectSO> OnGachaRolled;

    [SerializeField] private List<GachaObjectSO> pool = new();

    [SerializeField] private bool rollOnStart = true;

    private void Start()
    {
        if (rollOnStart) Roll();
    }

    private void Update()
    {
        // Press R to roll
        if (Input.GetKeyDown(KeyCode.O))
        {
            Roll();
        }
    }

    [ContextMenu("Roll Now")]
    public void Roll()
    {
        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning("GachaMachine: Pool is empty.");
            return;
        }

        var pick = pool[UnityEngine.Random.Range(0, pool.Count)];
        OnGachaRolled?.Invoke(pick);
    }
}
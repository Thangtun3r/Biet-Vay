using System;
using System.Collections.Generic;
using UnityEngine;

public class GachaMachine : MonoBehaviour
{
    public static event Action<GachaObjectSO> OnGachaRolled;

    [SerializeField] private List<GachaObjectSO> pool = new();

    private bool rollOnStart = false;

    public Animator gachaMachineAnimator;
    

    private void OnEnable()
    {
        GameManager.OnRollGacha += Roll;
    }

    private void OnDisable()
    {
        GameManager.OnRollGacha -= Roll;
    }

    private void Start()
    {
        if (rollOnStart) Roll();
    }

    public void Roll()
    {
        gachaMachineAnimator.SetTrigger("Roll");
    }

    public void HandleDisplayGachaItem()
    {
        var pick = pool[UnityEngine.Random.Range(0, pool.Count)];
        OnGachaRolled?.Invoke(pick);
    }
    
}
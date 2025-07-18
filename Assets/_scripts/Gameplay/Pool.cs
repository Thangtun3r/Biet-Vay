using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool : MonoBehaviour
{
    public enum PoolState
    {
        InSentence,
        InWordpool
    }

    [SerializeField]
    private PoolState poolState;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("detector"))
        {
            var wordOrder = other.GetComponent<IWordOrder>();
            if (wordOrder != null)
            {
                //wordOrder.OrderInPosition(poolState == PoolState.InSentence ? Words.WordState.InSentence : Words.WordState.InWordpool);
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Place this script on the SentencePool gameObject in the scene
public class SentenceChecker : MonoBehaviour
{
    public WordsPooling wordPool;
    
    private List<WordID> wordIDsList = new List<WordID>();
    private List<int> correctOrder;
    private List<int> userOrder;
    private bool isCorrect = false;

    private void OnEnable()
    {
        WordPoolManager.OnPoolCreated += GetCorrectOrder;
    }

    private void OnDisable()
    {
        WordPoolManager.OnPoolCreated -= GetCorrectOrder;
    }

    public void Check()
    {
        if (correctOrder == null || correctOrder.Count == 0 || transform.childCount == 0) return;
        userOrder = new List<int>();
        CurrentOrder();
        
        if (isMatched(userOrder, correctOrder))
        {
            isCorrect = true;
            Debug.Log("Correct Order!");
            wordPool.clearPool();
        }
        else
        {
            isCorrect = false;
            Debug.Log("Incorrect Order!");
        }
        {
            
        }
    }
    
    

    public void GetCorrectOrder(List<int> order)
    {
        correctOrder = order;
    }

    private void CurrentOrder()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            WordID wordID = child.GetComponent<WordID>();
            userOrder.Add(wordID.id);
        }
        Debug.Log("Current Order: " + string.Join(",", userOrder));
    }
    
    private bool isMatched(List<int> a, List<int> b)
    {
        if (a.Count != b.Count)
            return false;
        
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i] != b[i]) return false;
        }
        
        return true;
    }

    
    
}

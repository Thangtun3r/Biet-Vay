using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class SentenceChecker : MonoBehaviour
{
    public WordDisplayManager wordDisplayManager;
    public static event Action OnCheckCompleted;
    public static event Action OnCheckError;
    private List<OptionData> correctOrder;
    private List<string> userOrder;
    private bool isCorrect = false;

    
    private bool isResolved = false;
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

        userOrder = new List<string>();
        CurrentOrder();

        int matchedID = GetMatchingOptionID(userOrder, correctOrder);
        if (matchedID != -1)
        {
            isCorrect = true;
            wordDisplayManager?.SelectOptionByID(matchedID);
            OnCheckCompleted?.Invoke();
        }
        else
        {
            isCorrect = false;
            OnCheckError?.Invoke(); // <-- only fire once, here
        }
    }

    public void GetCorrectOrder(List<OptionData> order)
    {
        correctOrder = order;
    }

    private void CurrentOrder()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            WordID wordID = child.GetComponent<WordID>();
            userOrder.Add(wordID.originalWord);
        }
    }

    private int GetMatchingOptionID(List<string> userOrder, List<OptionData> options)
    {
        foreach (var option in options)
        {
            if (IsExactMatch(userOrder, option.words))
                return option.id;
        }
        return -1;
    }
    
    private bool IsExactMatch(List<string> a, List<string> b)
    {
        if (a.Count != b.Count)
            return false;

        for (int i = 0; i < a.Count; i++)
        {
            if (!string.Equals(a[i], b[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }
        return true;
    }

    
}
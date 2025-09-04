using System;
using System.Collections.Generic;
using UnityEngine;

public class WordsPooling : MonoBehaviour
{
    public GameObject wordPoolPrefab;
    public int poolSize = 10; 
    
    private List<GameObject> _wordPool = new List<GameObject>();


    private void OnEnable()
    {
        SentenceChecker.OnCheckCompleted += ClearPool;
    }

    private void OnDisable()
    {
        SentenceChecker.OnCheckCompleted -= ClearPool;
    }

    private void Awake()
    {
        PopulateThePool();
    }
    
    private void PopulateThePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewPooledObject();
        }
    }
    
    private GameObject CreateNewPooledObject()
    {
        GameObject obj = Instantiate(wordPoolPrefab, transform);
        obj.SetActive(false);
        _wordPool.Add(obj);
        return obj;
    }
    
    public GameObject GetPooledObject()
    {
        foreach (GameObject obj in _wordPool)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                obj.transform.SetParent(transform, false);
                return obj;
            }
        }
        return CreateNewPooledObject(); // Expand pool if none available
    }
    
    
    public void ClearPool()
    {
        foreach (GameObject obj in _wordPool)
        {
            if (obj.activeInHierarchy)
            {
                obj.SetActive(false);
                obj.transform.SetParent(transform, false);
            }
        }
    }
    
}
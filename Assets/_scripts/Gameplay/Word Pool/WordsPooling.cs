using System.Collections.Generic;
using UnityEngine;

public class WordsPooling : MonoBehaviour
{
    public GameObject wordPoolPrefab;
    public int poolSize = 10; 
    
    private List<GameObject> _wordPool = new List<GameObject>(); 
    
    private void Start()
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

    public void returnToPool(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform, false); 
    }
    
    public void clearPool()
    {
        foreach (GameObject obj in _wordPool)
        {
            if (obj.activeInHierarchy)
            {
                obj.SetActive(false);
            }
        }
    }
    
}
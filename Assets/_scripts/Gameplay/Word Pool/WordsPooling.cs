using System;
using System.Collections.Generic;
using UnityEngine;


public class WordsPooling : MonoBehaviour
{
    public GameObject wordPoolPrefab;
    public int poolSize = 10; 
    private List<GameObject> _wordPool = new List<GameObject>();
    
    private bool isResolved = true;
    
    private void OnEnable()
    {
        SentenceChecker.OnCheckCompleted += ClearPool;
        WordMarkup.OnReleaseBietVay += HandleReleaseBietVay;
        WordMarkup.OnBietVay += HandleBietVay;
        GameManager.OnExpand += HandleBietVay;
        GameManager.OnExpand += ClearPool;
        GameManager.OnBietvay += HandleBietVay;
        GameManager.OnResolveAnim += HandleReleaseBietVay;
        GameManager.OnNotBietvay += HandleReleaseBietVay;
    }

    private void OnDisable()
    {
        SentenceChecker.OnCheckCompleted -= ClearPool;
        WordMarkup.OnReleaseBietVay -= HandleReleaseBietVay;
        WordMarkup.OnBietVay -= HandleBietVay;
        GameManager.OnExpand -= HandleBietVay;
        GameManager.OnExpand -= ClearPool;
        GameManager.OnBietvay -= HandleBietVay;
        GameManager.OnResolveAnim -= HandleReleaseBietVay;
        GameManager.OnNotBietvay -= HandleReleaseBietVay;
    }

    private void HandleBietVay()
    {
        isResolved = false;
    }

    private void HandleReleaseBietVay()
    {
        Debug.Log("[WordsPooling] HandleReleaseBietVay called, setting isResolved to true");
        isResolved = true;
    }

    private string GetCaller()
    {
        // Gives you the method that invoked this handler
        var stackTrace = new System.Diagnostics.StackTrace();
        // 2 = caller of handler, 3 = deeper (you can adjust depth if needed)
        var frame = stackTrace.GetFrame(2);
        var method = frame?.GetMethod();
        return method != null ? $"{method.DeclaringType}.{method.Name}" : "Unknown";
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
            if (obj != null && !obj.activeInHierarchy)
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
        if (isResolved == true) return;
        
        Debug.Log($"[WordsPooling] ClearPool called (isResolved={isResolved})\n" +
                  $"Caller: {GetCaller()}");
        
        foreach (var obj in _wordPool)
        {
            if (obj != null && obj.activeInHierarchy)
            {
                obj.SetActive(false);
                obj.transform.SetParent(transform, false);
            }
        }
    }

    private float logTimer = 0f;

    private void Update()
    {
        Debug.Log($"[WordsPooling] isResolved = {isResolved}");
        logTimer += Time.deltaTime;

        if (logTimer >= 0.1f) // 0.1s = 1/10th second
        {
            
            logTimer = 0f;
        }
    }
}

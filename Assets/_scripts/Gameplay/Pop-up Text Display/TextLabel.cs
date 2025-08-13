using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class TextLabel : MonoBehaviour
{
    [SerializeField] private GameObject labelPrefab;
    [SerializeField] private float labelOffset = 1.5f; // Offset above the object for the label
    


    
    private void Start()
    {
        HandleLabel();
    }

    private void HandleLabel()
    {
        Vector3 position = transform.position + Vector3.up * labelOffset;
        
        GameObject labelTextPrefabs = Instantiate(labelPrefab,position, Quaternion.identity);
        labelTextPrefabs.transform.SetParent(transform);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneNames;


    private void Update()
    {
        HandleLoadScene(sceneNames);
    }
    
    private void HandleLoadScene(string sceneNames)
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            SceneManager.LoadScene(sceneNames);
        }
      
    }
}

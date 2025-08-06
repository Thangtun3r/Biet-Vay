using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn;
using Yarn.Unity;

public class YarnDialogueController : MonoBehaviour
{
    [Tooltip("The DialogueRunner component that runs the dialogue.")]
    public DialogueRunner Runner;
    
    private void OnEnable()
    {
        YarnDialogueEventBridge.OnYarnEventCalled += startNode;
    }
    private void OnDisable()
    {
        YarnDialogueEventBridge.OnYarnEventCalled -= startNode;
    }
    
    public void startNode(string nodeName)
    {
        if (Runner.IsDialogueRunning)
        {
            Debug.Log("Dialogue is running");
        }
        else
        {
            Runner.StartDialogue(nodeName);
        }
    }
}

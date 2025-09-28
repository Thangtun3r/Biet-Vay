using System;
using UnityEngine;
using Yarn.Unity;

public class YarnDialogueController : MonoBehaviour
{
    [Tooltip("The DialogueRunner component that runs the dialogue.")]
    public DialogueRunner Runner;
        
    private void OnEnable()
    {
        YarnDialogueEventBridge.OnYarnEventCalled += StartNode;
    }

    private void OnDisable()
    {
        YarnDialogueEventBridge.OnYarnEventCalled -= StartNode;
    }
    
    public void StartNode(string nodeName)
    {
        if (Runner == null || string.IsNullOrEmpty(nodeName))
            return;

        if (Runner.IsDialogueRunning)
        {
            Runner.Stop();  // cancel current line/options cleanly
        }

        Runner.StartDialogue(nodeName); // start fresh
    }
}
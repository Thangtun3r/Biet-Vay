using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn;
using Yarn.Unity;

public class YarnCommandTest : MonoBehaviour
{
    public DialogueRunner Runner;
    
    
    [YarnCommand("test_command")]
        public static void TestCommand()
        {
            Debug.Log("TestCommand executed!");
        }

    private void OnEnable()
    {
        YarnEventCaller.OnYarnEventCalled += startNode;
    }
    private void OnDisable()
    {
        YarnEventCaller.OnYarnEventCalled -= startNode;
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

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

    private void Update()
    {
        startNode();

    }

    private void startNode()
    {
        if (Runner.IsDialogueRunning)
        {
            Debug.Log("Dialogue is running");
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Runner.StartDialogue("first");
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Runner.StartDialogue("second");
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Runner.StartDialogue("third");
            }
        }
       
    }
}

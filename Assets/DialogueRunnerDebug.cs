using UnityEngine;
using Yarn.Unity;
using System;

public class YarnDebugTracer : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] DialogueRunner runner;

    [Header("Options")]
    [SerializeField] bool logStackTraceOnNodeStart = false;
    [SerializeField] bool logWhenAlreadyRunning = true;

    string currentNode;
    bool isRunning;

    void OnEnable() {
        if (runner == null) {
            Debug.LogWarning("[YarnDebug] No DialogueRunner assigned.", this);
            return;
        }

        runner.onDialogueStart.AddListener(OnDialogueStart);
        runner.onDialogueComplete.AddListener(OnDialogueComplete);
        runner.onNodeStart.AddListener(OnNodeStart);
        runner.onNodeComplete.AddListener(OnNodeComplete);

        if (runner.onUnhandledCommand != null)
            runner.onUnhandledCommand.AddListener(OnUnhandledCommand);
    }

    void OnDisable() {
        if (runner == null) return;
        runner.onDialogueStart.RemoveListener(OnDialogueStart);
        runner.onDialogueComplete.RemoveListener(OnDialogueComplete);
        runner.onNodeStart.RemoveListener(OnNodeStart);
        runner.onNodeComplete.RemoveListener(OnNodeComplete);
        if (runner.onUnhandledCommand != null)
            runner.onUnhandledCommand.RemoveListener(OnUnhandledCommand);
    }

    void OnDialogueStart() {
        if (logWhenAlreadyRunning && isRunning) {
            Debug.LogWarning("[YarnDebug] Dialogue START received while already running (double start?)");
        }
        isRunning = true;
        Debug.Log("[YarnDebug] Dialogue START");
    }

    void OnDialogueComplete() {
        Debug.Log("[YarnDebug] Dialogue COMPLETE");
        isRunning = false;
    }

    void OnNodeStart(string node) {
        currentNode = node;
        if (logStackTraceOnNodeStart) {
            Debug.Log($"[YarnDebug] Node START: {node}\n{Environment.StackTrace}");
        } else {
            Debug.Log($"[YarnDebug] Node START: {node}");
        }
    }

    void OnNodeComplete(string node) {
        Debug.Log($"[YarnDebug] Node COMPLETE: {node}");
    }

    void OnUnhandledCommand(string commandText) {
        Debug.LogWarning($"[YarnDebug] Unhandled Command: <<{commandText}>>");
    }
}

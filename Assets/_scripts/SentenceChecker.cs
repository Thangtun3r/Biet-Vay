using System;
using System.Collections.Generic;
using UnityEngine;

public class SentenceChecker : MonoBehaviour
{
    [Header("How many words must exist before checking?")]
    public int requiredChildren = 0;

    [Header("Optional Reset Link")]
    public WordPoolTrigger wordPackageRandomizer;

    // ✅ Event to notify when a correct sentence is formed
    public static event Action OnSentenceCorrect;             // :contentReference[oaicite:0]{index=0}
    // 🚫 Event to notify when the sentence is wrong
    public static event Action OnSentenceWrong;

    private void OnEnable()
    {
        WordPoolManager.OnSentenceCreated += UpdateRequiredChildren;
    }

    private void OnDisable()
    {
        WordPoolManager.OnSentenceCreated -= UpdateRequiredChildren;
    }

    private void UpdateRequiredChildren(int totalWords)
    {
        requiredChildren = totalWords;
        Debug.Log($"🔄 SentenceChecker updated requiredChildren = {requiredChildren}");
    }

    /// <summary>
    /// Checks only the active children under this transform, in order,
    /// and returns true if each WordID.id matches its 1-based index.
    /// </summary>
    private bool CheckHierarchyOrder()
    {
        // Gather only active children
        List<Transform> activeChildren = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.activeSelf)
                activeChildren.Add(child);
        }

        int childCount = activeChildren.Count;
        if (childCount < requiredChildren)
        {
            Debug.Log($"ℹ️ Not enough words yet ({childCount}/{requiredChildren}). Waiting...");
            return false;
        }

        bool isCorrect = true;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = activeChildren[i];
            WordID wordID = child.GetComponent<WordID>();
            int expectedID = i + 1;

            if (wordID == null)
            {
                Debug.LogWarning($"Child {child.name} has no WordID component!");
                isCorrect = false;
                continue;
            }

            if (wordID.id != expectedID)
            {
                Debug.Log($"❌ Mismatch at active index {i} (expected {expectedID}, got {wordID.id}) → word: {wordID.word}");
                isCorrect = false;
            }
        }

        if (isCorrect)
            Debug.Log("✅ All active words are in the correct order!");

        return isCorrect;
    }

    /// <summary>
    /// Call this from your UI Button (e.g. “Check and Reset”).
    /// </summary>
    public void CheckAndResetIfCorrect()
    {
        if (CheckHierarchyOrder())
        {
            Debug.Log("🎉 Correct! Broadcasting event & resetting pools...");
            OnSentenceCorrect?.Invoke();

            // ✅ Priority check to prevent conflicting tween calls
            if (!FinalActPsudo.ForceFinalActPriority)
            {
                if (PopOutRectTween.Instance != null)
                    PopOutRectTween.Instance.ToggleOn();  // Normal behavior
            }
            else
            {
                Debug.Log("⚡ Skipped SentenceChecker tween because FinalAct has priority");
            }

            wordPackageRandomizer?.ClearPool();
        }
        else
        {
            Debug.Log("🚫 Incorrect order! Cannot reset yet.");
            OnSentenceWrong?.Invoke();
        }
    }


    private void Update()
    {
        // Optional debug shortcut: press Space to run the full check & reset
        if (Input.GetKeyDown(KeyCode.Space))
            CheckAndResetIfCorrect();
    }
}

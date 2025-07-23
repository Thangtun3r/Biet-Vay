using UnityEngine;

public class SentenceChecker : MonoBehaviour
{
    [Header("How many words must exist before checking?")]
    public int requiredChildren = 0;

    [Header("Optional Reset Link")]
    public WordPackageRandomizer wordPackageRandomizer; // Reference to reset if correct

    private void OnEnable()
    {
        // ✅ Subscribe to WordPoolManager event
        WordPoolManager.OnSentenceCreated += UpdateRequiredChildren;
    }

    private void OnDisable()
    {
        // ✅ Unsubscribe when disabled to avoid memory leaks
        WordPoolManager.OnSentenceCreated -= UpdateRequiredChildren;
    }

    private void UpdateRequiredChildren(int totalWords)
    {
        requiredChildren = totalWords;
        Debug.Log($"🔄 SentenceChecker updated requiredChildren = {requiredChildren}");
    }

    /// <summary>
    /// Checks the hierarchy order and returns true if correct
    /// </summary>
    private bool CheckHierarchyOrder()
    {
        int childCount = transform.childCount;

        if (childCount < requiredChildren)
        {
            Debug.Log($"ℹ️ Not enough words yet ({childCount}/{requiredChildren}). Waiting...");
            return false;
        }

        bool isCorrect = true;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            WordID wordID = child.GetComponent<WordID>();

            if (wordID == null)
            {
                Debug.LogWarning($"Child {child.name} has no WordID component!");
                isCorrect = false;
                continue;
            }

            int expectedID = i + 1; // 1-based order
            if (wordID.id != expectedID)
            {
                Debug.Log($"❌ Mismatch at index {i} (expected {expectedID}, got {wordID.id}) → word: {wordID.word}");
                isCorrect = false;
            }
        }

        if (isCorrect)
        {
            Debug.Log("✅ All words are in the correct hierarchy order (1-based)!");
        }

        return isCorrect;
    }

    /// <summary>
    /// Call this from UI Button instead of directly resetting
    /// </summary>
    public void CheckAndResetIfCorrect()
    {
        bool correct = CheckHierarchyOrder();

        if (correct)
        {
            Debug.Log("🎉 Correct! Resetting pools...");
            if (wordPackageRandomizer != null)
            {
                wordPackageRandomizer.ClearPool();
            }
        }
        else
        {
            Debug.Log("🚫 Incorrect order! Cannot reset yet.");
        }
    }

    private void Update()
    {
        // Optional debug key for quick check
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckHierarchyOrder();
        }
    }
}

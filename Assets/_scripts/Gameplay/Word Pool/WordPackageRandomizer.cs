/*using UnityEngine;

public class WordPackageRandomizer : MonoBehaviour
{
    [Header("References")]
    public WordPoolManager wordPoolManager;
    public WordsPooling wordPooling;
    public WordsPooling visualPooling;

    [Header("Settings")]
    public bool clearBeforePour = true; // optional: auto clear before pouring new data

    /// <summary>
    /// Another script can call this to instantly pour a specific WordsPackage.
    /// </summary>
    public void LoadAndPour(WordsPackage package)
    {
        if (package == null)
        {
            Debug.LogWarning("[WordPackageRandomizer] Tried to pour a null package!");
            return;
        }

        if (clearBeforePour)
        {
            ClearPool();
        }

        wordPoolManager.CreateSentence(package);
        Debug.Log($"[WordPackageRandomizer] Poured package: {package.name}");
    }

    /// <summary>
    /// Clears both the logic and visual pools.
    /// </summary>
    public void ClearPool()
    {
        if (wordPooling != null) wordPooling.clearPool();
        if (visualPooling != null) visualPooling.clearPool();
    }
}*/
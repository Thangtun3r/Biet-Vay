using System;
using UnityEngine;

public class WordPoolManager : MonoBehaviour
{
    [Header("Word Pooling")]
    public WordsPooling wordPooling;

    /// <summary>
    /// Fired every time we pull a WordID prefab out of the pool and set it up.
    /// </summary>
    public static event Action<int, string, Transform> OnWordCreated;

    public void CreateSentence(WordsPackage wordsPackage)
    {
        foreach (var template in wordsPackage.words)
        {
            // 1. Get a pooled prefab
            GameObject pooledObj = wordPooling.GetPooledObject();
            WordID wordComponent = pooledObj.GetComponent<WordID>();

            // 2. Initialize the logic object
            wordComponent.word = template.word;
            wordComponent.id   = template.id;
            wordComponent.wordText.text = wordComponent.word;

            // 3. Broadcast it
            OnWordCreated?.Invoke(wordComponent.id,
                wordComponent.word,
                pooledObj.transform);
        }
    }
}
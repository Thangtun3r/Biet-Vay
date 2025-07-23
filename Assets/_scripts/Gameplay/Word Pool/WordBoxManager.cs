using System.Collections.Generic;
using UnityEngine;

public class WordPoolManager : MonoBehaviour
{
    [Header("Word Pooling")]
    public WordsPooling wordPooling;

    // ✅ Event when each word is created (already exists)
    public static event System.Action<int, string, Transform> OnWordCreated;

    // ✅ NEW: Event when the whole sentence is finished spawning
    public static event System.Action<int> OnSentenceCreated; 
    // int = total number of words created

    public void CreateSentence(WordsPackage wordsPackage)
    {
        // Temporary list to hold created objects
        List<GameObject> createdObjects = new List<GameObject>();

        foreach (var template in wordsPackage.words)
        {
            // Get a pooled prefab
            GameObject pooledObj = wordPooling.GetPooledObject();
            WordID wordComponent = pooledObj.GetComponent<WordID>();

            // Initialize
            wordComponent.word = template.word;
            wordComponent.id = template.id;
            wordComponent.wordText.text = wordComponent.word;

            // Add to list
            createdObjects.Add(pooledObj);

            // Broadcast individual word creation
            OnWordCreated?.Invoke(wordComponent.id, wordComponent.word, pooledObj.transform);
        }

        // ✅ Shuffle the created objects
        ShuffleList(createdObjects);

        // ✅ Randomize their sibling order
        for (int i = 0; i < createdObjects.Count; i++)
        {
            createdObjects[i].transform.SetSiblingIndex(i);
        }

        // ✅ Broadcast total sentence word count
        OnSentenceCreated?.Invoke(createdObjects.Count);
        Debug.Log($"✅ Sentence created with {createdObjects.Count} words.");
    }

    // Fisher-Yates shuffle
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}

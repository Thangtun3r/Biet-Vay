using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WordPoolManager : MonoBehaviour
{
    [Header("Word Pooling")]
    public WordsPooling wordPooling;
    
    public static WordPoolManager Instance;
    public static event System.Action<int, string, Transform> OnWordCreated;
    
    public static event System.Action<int> OnSentenceCreated;

    private void Awake()
    {
        Instance = this;
        
    }

    public void CreateSentenceFromText(string sentence)
    {
        List<GameObject> createdObjects = new List<GameObject>();

        var chunks = ParseChunksWithID(sentence);

        foreach (var chunk in chunks)
        {
            GameObject pooledObj = wordPooling.GetPooledObject();
            WordID wordComponent = pooledObj.GetComponent<WordID>();

            wordComponent.word = chunk.text;
            wordComponent.id = chunk.id;
            wordComponent.wordText.text = chunk.text;

            createdObjects.Add(pooledObj);

            OnWordCreated?.Invoke(chunk.id, chunk.text, pooledObj.transform);
        }

        ShuffleList(createdObjects);

        for (int i = 0; i < createdObjects.Count; i++)
        {
            createdObjects[i].transform.SetSiblingIndex(i);
        }
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
    
    private List<WordChunkData> ParseChunksWithID(string input)
    {
        var result = new List<WordChunkData>();
        if (string.IsNullOrWhiteSpace(input))
            return result;

        var split = input.Split('/', System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < split.Length; i++)
        {
            result.Add(new WordChunkData { id = i, text = split[i].Trim() });
        }
        return result;
    }
    private class WordChunkData
    {
        public int id;
        public string text;
    }

}

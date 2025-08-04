using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public struct OptionData {
    public int id;
    public List<string> words;
}

public class WordPoolManager : MonoBehaviour
{
    [Header("Word Pooling")]
    public WordsPooling wordPooling;

    public static WordPoolManager Instance;
    public static event Action<int, string, Transform> OnWordCreated;
    public static event Action<List<OptionData>> OnPoolCreated;

    [Header("Options Presenter Section")]
    private List<OptionData> OptionsCollection = new List<OptionData>();
    private HashSet<string> spawnedWords = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate WordPoolManager found! Destroying this one: " + gameObject.name);
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void ResetPool()
    {
        OptionsCollection.Clear();
        spawnedWords.Clear();
    }

    public void CreateSentenceFromText(string sentence, int optionID)
    {
        List<GameObject> createdObjects = new List<GameObject>();
        var chunks = ParseChunksWithID(sentence);
        var singleOptionsOrder = new List<string>();

        foreach (var chunk in chunks)
        {
            singleOptionsOrder.Add(chunk.text);

            if (spawnedWords.Contains(chunk.text))
                continue; // prevent duplicate visual word

            GameObject pooledObj = wordPooling.GetPooledObject();
            WordID wordComponent = pooledObj.GetComponent<WordID>();

            wordComponent.word = chunk.text;
            wordComponent.id = chunk.id;
            wordComponent.wordText.text = chunk.text;

            createdObjects.Add(pooledObj);
            spawnedWords.Add(chunk.text);

            OnWordCreated?.Invoke(chunk.id, chunk.text, pooledObj.transform);
        }

        OptionsCollection.Add(new OptionData {
            id = optionID,
            words = singleOptionsOrder
        });

        OnPoolCreated?.Invoke(OptionsCollection);

        ShuffleList(createdObjects);
        for (int i = 0; i < createdObjects.Count; i++)
        {
            createdObjects[i].transform.SetSiblingIndex(i);
        }
    }

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
        if (string.IsNullOrWhiteSpace(input)) return result;

        var split = input.Split('/', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < split.Length; i++)
        {
            result.Add(new WordChunkData { id = i, text = split[i].Trim() });
        }
        return result;
    }

    private class WordChunkData {
        public int id;
        public string text;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
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
    private readonly List<OptionData> OptionsCollection = new List<OptionData>();

    // How many copies of each word have we spawned globally so far?
    private readonly Dictionary<string, int> spawnedCountByWord =
        new Dictionary<string, int>(StringComparer.Ordinal);

    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Debug.LogWarning("Duplicate WordPoolManager found! Destroying this one: " + gameObject.name);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ResetPool()
    {
        OptionsCollection.Clear();
        spawnedCountByWord.Clear();
    }

    public void CreateSentenceFromText(string sentence, int optionID)
    {
        if (string.IsNullOrWhiteSpace(sentence)) return;

        // Parse and keep the original per-option order
        var chunks = ParseChunksWithID(sentence);
        var singleOptionsOrder = new List<string>(chunks.Count);
        foreach (var c in chunks) singleOptionsOrder.Add(c.text);

        // Count duplicates in THIS option only
        var localCounts = CountLocalOccurrences(chunks);

        // Only spawn the delta compared to what's already globally available
        var createdObjects = new List<GameObject>();
        foreach (var kvp in localCounts)
        {
            string word = kvp.Key;
            int localNeed = kvp.Value;

            spawnedCountByWord.TryGetValue(word, out int spawnedSoFar);
            int toSpawn = localNeed - spawnedSoFar;
            if (toSpawn <= 0) continue;

            // Use this option's IDs for this word (so each copy still has a sensible id)
            var idsForWord = GetIdsForWord(chunks, word);
            for (int i = 0; i < toSpawn && i < idsForWord.Count; i++)
            {
                GameObject pooledObj = wordPooling.GetPooledObject();
                var wordComponent = pooledObj.GetComponent<WordID>();

                wordComponent.word = word;
                wordComponent.id = idsForWord[i];
                wordComponent.wordText.text = word;

                createdObjects.Add(pooledObj);
                OnWordCreated?.Invoke(wordComponent.id, word, pooledObj.transform);
            }

            spawnedCountByWord[word] = spawnedSoFar + toSpawn;
        }

        // Record this option's sequence (including local duplicates)
        OptionsCollection.Add(new OptionData {
            id = optionID,
            words = singleOptionsOrder
        });
        OnPoolCreated?.Invoke(OptionsCollection);

        // Shuffle only the newly spawned visuals
        ShuffleList(createdObjects);
        for (int i = 0; i < createdObjects.Count; i++)
            createdObjects[i].transform.SetSiblingIndex(i);
    }

    // --- helpers ---

    private static Dictionary<string,int> CountLocalOccurrences(List<WordChunkData> chunks)
    {
        var counts = new Dictionary<string,int>(StringComparer.Ordinal);
        for (int i = 0; i < chunks.Count; i++)
        {
            string w = chunks[i].text;
            counts[w] = counts.TryGetValue(w, out int c) ? c + 1 : 1;
        }
        return counts;
    }

    private static List<int> GetIdsForWord(List<WordChunkData> chunks, string word)
    {
        var ids = new List<int>();
        for (int i = 0; i < chunks.Count; i++)
            if (chunks[i].text == word) ids.Add(chunks[i].id);
        return ids;
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
            result.Add(new WordChunkData { id = i, text = split[i].Trim() });

        return result;
    }

    private class WordChunkData {
        public int id;
        public string text;
    }
}

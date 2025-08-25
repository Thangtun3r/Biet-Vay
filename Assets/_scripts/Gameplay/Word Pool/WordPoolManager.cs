using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Yarn.Markup;

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

    //Need to clean the pool if not old words will persist. Very important.
    public void ResetPool()
    {
        OptionsCollection.Clear();
        spawnedCountByWord.Clear();
    }

    
    /// <summary>
    /// This large method is responsible for creating words from the parsed text.
    /// To-do: Break it down into smaller methods if needed. This shit is too coupled already.
    /// </summary>
    public void CreateSentenceFromText(MarkupParseResult parsed, int optionID)
    {
        string sentence = parsed.Text;
        if (string.IsNullOrWhiteSpace(sentence)) return;

        var chunks = ParseChunksWithID(sentence);
        var singleOptionsOrder = new List<string>(chunks.Count);
        foreach (var c in chunks) singleOptionsOrder.Add(c.text);
        
        var localCounts = CountLocalOccurrences(chunks);
        
        var createdObjects = new List<GameObject>();
        foreach (var kvp in localCounts)
        {
            string word = kvp.Key;
            int localNeed = kvp.Value;

            spawnedCountByWord.TryGetValue(word, out int spawnedSoFar);
            int toSpawn = localNeed - spawnedSoFar;
            if (toSpawn <= 0) continue;
            var idsForWord = GetIdsForWord(chunks, word);
            for (int i = 0; i < toSpawn && i < idsForWord.Count; i++)
            {
                
                GameObject pooledObj = wordPooling.GetPooledObject();
                var wordComponent = pooledObj.GetComponent<WordID>();
                var wordWMarkup = pooledObj.GetComponent<WordMarkup>();

                wordComponent.word = word;
                wordComponent.id = idsForWord[i];
                wordComponent.wordText.text = word;
                

                createdObjects.Add(pooledObj);
                OnWordCreated?.Invoke(wordComponent.id, word, pooledObj.transform);
                
                ApplyAttributesToWord(wordComponent,wordWMarkup, chunks, parsed);


            }

            spawnedCountByWord[word] = spawnedSoFar + toSpawn;
            
        }
        
        OptionsCollection.Add(new OptionData 
        {
            id = optionID,
            words = singleOptionsOrder
        });
        OnPoolCreated?.Invoke(OptionsCollection);

        // Shuffle only the newly spawned visuals
        ShuffleList(createdObjects);
        for (int i = 0; i < createdObjects.Count; i++)
            createdObjects[i].transform.SetSiblingIndex(i);
    }
    
    
    
    
    
    

     // ======= Helpers ========================================================================================================

    /// <summary>
    /// This will check if two ranges overlap.
    /// If overlap they will allow, the attribute to be applied.
    /// </summary>
     private static bool Overlaps(int aStart, int aEnd, int bStart, int bEnd) 
     {
         return aStart < bEnd && bStart < aEnd;
     }
    
     
    /// <summary>
    /// This is where we apply the parsed attributes to the WordMarkup component of each word.
    /// </summary>
    private void ApplyAttributesToWord(
        WordID wordComponent,
        WordMarkup wordMarkup,
        List<WordChunkData> chunks,
        MarkupParseResult parsed
    ) {
        var chunk = chunks.Find(c => c.id == wordComponent.id);
        if (chunk == null) return;

        foreach (var attr in parsed.Attributes) {
            int attrStart = attr.Position;
            int attrEnd   = attr.Position + attr.Length;
            if (!Overlaps(attrStart, attrEnd, chunk.start, chunk.end))
                continue;

            if (attributeHandlers.TryGetValue(attr.Name, out var handler)) 
            {
                handler(wordMarkup, attr);
            }
        }
    }

    
    /// <summary>
    /// This part meant to prevent over-spawning of the same word within a single option.
    /// </summary>
    /// <param name="chunks"></param>
    /// <returns></returns>
     
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

    
    
    
    
    /// <summary>
    /// This chunk of method handles how the input string is parsed into chunks while also recording their IDs and positions.
    /// Then it will return the information as a list of WordChunkData objects.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private List<WordChunkData> ParseChunksWithID(string input) 
    {
        var result = new List<WordChunkData>();
        if (string.IsNullOrWhiteSpace(input)) return result;

        int id = 0;
        int index = 0;
        var split = input.Split('/', StringSplitOptions.RemoveEmptyEntries);

        foreach (var token in split) {
            string trimmed = token.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            int start = input.IndexOf(trimmed, index, StringComparison.Ordinal);
            int end = start + trimmed.Length;
            index = end;

            result.Add(new WordChunkData {
                id = id++,
                text = trimmed,
                start = start,
                end = end
            });
        }

        return result;
    }

    
    /// <summary>
    /// Add more custom attribute handlers here.
    /// </summary>
    private readonly Dictionary<string, Action<WordMarkup, MarkupAttribute>> 
        attributeHandlers = new Dictionary<string, Action<WordMarkup, MarkupAttribute>>(StringComparer.OrdinalIgnoreCase)
        {
            ["switch"] = (wordMarkup, attr) =>
            {
                if (attr.Properties.TryGetValue("word", out var value)) {
                    wordMarkup.isSwitch = true;
                    wordMarkup.switchWord = value.StringValue;
                }
            },
            ["bietvay"] = (wordMarkup, attr) =>
            {
                    wordMarkup.isBietVay = true;
            }
        };
    

    private class WordChunkData {
        public int id;
        public string text;
        public int start;
        public int end; 
    }
}

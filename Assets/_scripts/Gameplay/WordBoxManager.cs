using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordPoolManager : MonoBehaviour
{
    public WordsTemplate[] wordsTemplate;
    public GameObject wordPrefab;
    public int wordID;

    private void Start()
    {
        CreateWords();
    }

    private void CreateWords()
    {
        foreach (var wordTemplate in wordsTemplate)
        {
            GameObject wordObject = Instantiate(wordPrefab, transform);
            WordID wordIDComponent = wordObject.GetComponent<WordID>();
            if (wordIDComponent != null)
            {
                wordIDComponent.word = wordTemplate.word;
                wordIDComponent.id = this.wordID++;
            }
        }
    }
}

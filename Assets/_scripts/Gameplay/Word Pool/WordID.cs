using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WordID : MonoBehaviour
{
    public string originalWord;
    public string word;
    public int id;
    public TextMeshProUGUI wordText;

    private void Start()
    {
        word = originalWord;
    }
    
    public void AssignVisualWord(string word)
    {
        this.word = word;
        wordText.text = word;
    }
    

    
}

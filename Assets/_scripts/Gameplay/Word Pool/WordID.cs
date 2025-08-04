using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WordID : MonoBehaviour
{
    public string word;
    public int id;
    private string defaulWord; // Store the default word to reset later
    public TextMeshProUGUI wordText;
    
    
    private void AssigningWord()
    {
        wordText.text = word;
    }

    private void ResetWord()
    {
        word = "";
        AssigningWord();
    }

    private void OnDisable()
    {
        ResetWord();
    }
}

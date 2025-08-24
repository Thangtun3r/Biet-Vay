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
    public bool hasMarkup;
    public TextMeshProUGUI wordText;
    public string switchWord;


    private void OnEnable()
    {
        Switcher.OnSwitch += SwitchWord;
    }
    

    private void SwitchWord()
    {
        if (hasMarkup)
        {
            AssigningWord();
        }
        
    }

    private void AssigningWord()
    {
        wordText.text = word;
    }

    private void ResetWord()
    {
        word = "";
        AssigningWord();
    }

    
}

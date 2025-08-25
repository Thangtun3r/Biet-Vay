using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WordID : MonoBehaviour
{
    public string word;
    public int id;
    public TextMeshProUGUI wordText;
    
    

    public void AssigningWord(string word)
    {
        this.word = word;
        wordText.text = word;
    }

    
}

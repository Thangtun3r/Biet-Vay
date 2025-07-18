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

    private void Start()
    {
        AssigningWord();
    }

    private void AssigningWord()
    {
        wordText.text = word;
    }
}

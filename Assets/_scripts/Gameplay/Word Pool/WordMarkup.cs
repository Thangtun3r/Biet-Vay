using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordMarkup : MonoBehaviour
{
    public string switchWord;
    public bool isSwitch = false;
    public bool isBietVay = false;
    
    private WordID _wordID;
    private string _originalWord;
    
    
    
    
    
    

    public static event Action onBietVay;


    private void Start()
    {
        _wordID = GetComponent<WordID>();
        _originalWord = _wordID.word;
    }


    private void BietVay()
    {
        onBietVay.Invoke();
    }


    private void Switch()
    {
        _wordID.AssigningWord(switchWord);
    }
    private void ResetWord()
    {
        _wordID.AssigningWord(_originalWord);
    }
}

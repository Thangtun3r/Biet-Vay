using System;
using System.Collections;
using System.Collections.Generic;
using Codice.CM.Client.Differences;
using Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

public class WordMarkup : MonoBehaviour,IBietVaySentenceChecker
{
    
    public WordsPooling visualPooling;  
    public string switchWord;
    public bool isSwitch = false;
    public bool isBietVay = false;
    public bool isVisual = false;
    
    private WordID _wordID;
    public string _originalWord;
    public Words _words;
    
    
    public static event Action OnBietVay;
    public static event Action OnReleaseBietVay;
    

    private void OnEnable()
    {
        if (!isVisual && _words != null)
        {
            // _words.BeganDrag += BietVay;
            // _words.Dragged += BietVay;
            //_words.EndedDrag += ReleaseBietVay;
            //_words.PointerUpped += ReleaseBietVay;
        }
        WordMarkup.OnBietVay += Switch;
        WordMarkup.OnReleaseBietVay += ResetWord;
      
    }

    private void OnDisable()
    {
        if (!isVisual && _words != null)
        {
            
            // _words.BeganDrag -= BietVay;
            // _words.Dragged -= BietVay;
            // _words.EndedDrag -= ReleaseBietVay;
            // _words.PointerUpped -= ReleaseBietVay;
        }

        WordMarkup.OnBietVay -= Switch;
        WordMarkup.OnReleaseBietVay -= ResetWord;
    }

    public void RaiseBietVayEvent()
    {
        if (isBietVay)
        {
            OnBietVay?.Invoke();
        }
        
    }
    
    public void RaiseReleaseEvent()
    {
        if (isBietVay)
        {
            OnReleaseBietVay?.Invoke();
        }
    }
    
    private void Start()
    {
        _wordID = GetComponent<WordID>();
        _originalWord = _wordID.word;
    }

    

    private void Switch()
    {
        if (isSwitch)
        {
            _wordID.AssignVisualWord(switchWord);
        }
        
    }
    private void ResetWord()
    {
        _wordID.AssignVisualWord(_originalWord);
    }
}

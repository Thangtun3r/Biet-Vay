using System;
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
        WordMarkup.OnBietVay += Switch;
        WordMarkup.OnReleaseBietVay += ResetWord;
       
        
       
    }

    private void OnDisable()
    {
        WordMarkup.OnBietVay -= Switch;
        WordMarkup.OnReleaseBietVay -= ResetWord;
        
        // we flush the variables to avoid it being carried over to the next word
        _originalWord = string.Empty;
        switchWord = string.Empty;
        isSwitch = false;
        isBietVay = false;
        isVisual = false;
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
        if (!isSwitch || _wordID == null || string.IsNullOrEmpty(switchWord)) return;
        _wordID.AssignVisualWord(switchWord);
        // We need to assign new original words to avoid the old one being carried over
        _wordID = GetComponent<WordID>();
        _originalWord = _wordID.originalWord;
    }

    private void ResetWord()
    {
        if (_wordID == null || string.IsNullOrEmpty(_originalWord)) return;
        _wordID.AssignVisualWord(_originalWord);
    }
}

using System;
using Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

public class WordMarkup : MonoBehaviour,IBietVaySentenceChecker
{
    
    public WordsPooling visualPooling;  
    public string switchWord;
    public bool isSwitch = false;
    public bool isBietVay = false;
    public bool isFadeTrigger = false;
    public bool isVisual = false;
    public bool isBeingFade = false;
    
    public bool isFriction = false;
    public bool isFade = false;
    
    private WordID _wordID;
    public string _originalWord;
    public Words _words;
    
    public FrictionWord frictionWord;  // Drag the FrictionWord object in the inspector

    public FadeWordCommander fadeWord;
    private VisualToLogic visualToLogic;

    
    
    public static event Action OnBietVay;
    public static event Action OnReleaseBietVay;
    //fade word event
    public static event Action OnFadeWord;
    public static event Action OnReleaseFadeWord;
    //sproite change event
    public event Action<int> OnBietVaySpriteChange;
    
    private void Awake()
    {
        _wordID = GetComponent<WordID>();
        _originalWord = _wordID.word;
        frictionWord = GetComponent<FrictionWord>();
        fadeWord = GetComponent<FadeWordCommander>();
        visualToLogic = GetComponent<VisualToLogic>();
    }

    private void OnEnable()
    {
        WordMarkup.OnBietVay += Switch;
        WordMarkup.OnReleaseBietVay += ResetWord;
        
        //WORDFADE
        WordMarkup.OnFadeWord += WordFade;
        WordMarkup.OnReleaseFadeWord += ResetWordFade;

    }
    
    private void OnDisable()
    {
        WordMarkup.OnBietVay -= Switch;
        WordMarkup.OnReleaseBietVay -= ResetWord;
        
        //WORDFADE
        WordMarkup.OnFadeWord -= WordFade;
        WordMarkup.OnReleaseFadeWord -= ResetWordFade;
        
        // we flush the variables to avoid it being carried over to the next word
        _originalWord = string.Empty;
        switchWord = string.Empty;
        isSwitch = false;
        isBietVay = false;
        isVisual = false;
        OnBietVaySpriteChange?.Invoke(0);
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
    



    private void Update()
    {
        UpdateFadeLogic();
        UpdateFrictionState();
        if (isBietVay)
        {
            FlagBietvay();
        }
    }

    private void UpdateFadeLogic()
    {
        if (frictionWord != null)
        {
            fadeWord.isFadeWord = isFade;
        }
    }
    
    private void Switch()
    {
        if (!isSwitch || _wordID == null || string.IsNullOrEmpty(switchWord)) return;
        _wordID.AssignVisualWord(switchWord);
        OnBietVaySpriteChange?.Invoke(1);
        // We need to assign new original words to avoid the old one being carried over
        _wordID = GetComponent<WordID>();
        _originalWord = _wordID.originalWord;
    }

    private void ResetWord()
    {
        if (_wordID == null || string.IsNullOrEmpty(_originalWord)) return;
        _wordID.AssignVisualWord(_originalWord);
        OnBietVaySpriteChange?.Invoke(0);
    }
    
    
    //friction word logic
    private void UpdateFrictionState()
    {
        if (frictionWord != null)
        {
            frictionWord.isFriction = isFriction;
        }
    }
    
    public void StopFriction()
    {
        isFriction = false;
    }

    public void FlagBietvay()
    {
        Debug.Log("this is biet vay");
        OnBietVaySpriteChange?.Invoke(1);
    }
    
    public void ResetSprite()
    {
        OnBietVaySpriteChange?.Invoke(0);
    }
    
    
    //Fade logic 
    public void RaiseFadeWord()
    {
        if (isFadeTrigger)
        {
            OnFadeWord?.Invoke();
        }
    }
    
    public void LowerFadeWord()
    {
        if (isFadeTrigger)
        {
            OnReleaseFadeWord?.Invoke();
        }
    }

    private void WordFade()
    {
        if(!isBeingFade) return;
        fadeWord.TriggerFadeWord();
    }
    
    private void ResetWordFade()
    {
        if(!isBeingFade) return;
        fadeWord.ReleaseFadeWord();
        
    }
}

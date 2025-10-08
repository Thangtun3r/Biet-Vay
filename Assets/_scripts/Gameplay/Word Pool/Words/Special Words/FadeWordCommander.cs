using System;
using UnityEngine;
using Gameplay;

public class FadeWordCommander : MonoBehaviour
{
    public VisualToLogic visualToLogic;
    public static event Action OnFadeWordEvent;
    public event Action<int> OnFadeWord;

    public bool isFadeWord;

    private Words _words;
    private Transform _originalParent; // store where it came from

    private void Awake()
    {
        _words = GetComponent<Words>();
        _originalParent = transform.parent; // remember its original pool
    }

    
    private void Update()
    {
        if (isFadeWord)
        {
            _words.enabled = false;
            OnFadeWord?.Invoke(2);
        }
    }

    public void TriggerFadeWord()
    {
        isFadeWord = true;
        _words.enabled = false;
        OnFadeWord?.Invoke(2);
        OnFadeWordEvent?.Invoke();
        ReturnToOriginalParent();
    }

    public void ReleaseFadeWord()
    {
        isFadeWord = false;
        _words.enabled = true;
        OnFadeWord?.Invoke(0);
    }
    private void ReturnToOriginalParent()
    {
        if (_originalParent == null) return;
        transform.SetParent(_originalParent, worldPositionStays: false);
        transform.SetAsLastSibling();
    }
}
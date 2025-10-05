using System;
using UnityEngine;
using Gameplay;

public class FadeWordCommander : MonoBehaviour
{
    // Event the trigger will listen to
    public event Action<int> OnFadeWord;

    [Header("Logic")]
    [SerializeField] private Words _words;

    [Header("Who tells us the current visual word?")]
    [SerializeField] private VisualToLogic visualToLogic; // assign in Inspector

    // Current wired worker
    private GameObject visualWordObject;
    private FadeWordTrigger fadeWordTrigger;

    [Header("State")]
    public bool isFadeWord;

    private void Awake()
    {
        if (_words == null) _words = GetComponent<Words>();
    }

    private void OnEnable()
    {
        if (visualToLogic != null)
            visualToLogic.OnVisualWordSet += HandleSetVisualWord;

        if (_words != null)
            _words.enabled = true;
    }

    private void OnDisable()
    {
        if (visualToLogic != null)
            visualToLogic.OnVisualWordSet -= HandleSetVisualWord;

        UnwireWorker();
    }

    // ----- This is the key wiring method you asked for -----
    private void HandleSetVisualWord(GameObject visualWord)
    {
        // Unwire previous worker (if any)
        UnwireWorker();

        visualWordObject = visualWord;
        fadeWordTrigger = visualWordObject != null
            ? visualWordObject.GetComponent<FadeWordTrigger>()
            : null;

        if (fadeWordTrigger != null)
        {
            // Wire the worker (trigger) to our instance event
            OnFadeWord += fadeWordTrigger.HandleFadeWord;
        }
        else
        {
            Debug.LogWarning("FadeWordCommander: FadeWordTrigger not found on visual word object.", this);
        }
    }

    private void UnwireWorker()
    {
        if (fadeWordTrigger != null)
        {
            OnFadeWord -= fadeWordTrigger.HandleFadeWord;
            fadeWordTrigger = null;
        }
        visualWordObject = null;
    }

    private void Update()
    {
        if (isFadeWord)
        {
            OnFadeWord?.Invoke(2);
            if (_words != null) _words.enabled = false;
            // prevent spamming every frame unless you want continuous
            isFadeWord = false;
        }
    }

    // Public APIs you can call from anywhere
    public void TriggerFadeWord()
    {
        isFadeWord = true;
        OnFadeWord?.Invoke(2);
        if (_words != null) _words.enabled = false;
    }

    public void ReleaseFadeWord()
    {
        isFadeWord = false;
        if (_words != null) _words.enabled = true;
        OnFadeWord?.Invoke(0);
    }
}

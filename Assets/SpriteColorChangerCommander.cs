using System; // required for Action<T>
using UnityEngine;

public class SpriteColorChangerCommander : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VisualToLogic visualToLogic; // assign in Inspector or find at runtime

    private WordMarkup wordMarkup;
    private GameObject visualWordObject;
    private SpriteColorChanger spriteColorChanger;
    private FadeWordCommander fadeWord;

    public event Action<int> OnRequestTypeChange;


    private void Awake()
    {
        wordMarkup = GetComponent<WordMarkup>();
        fadeWord = GetComponent<FadeWordCommander>();
    }

    private void OnEnable()
    {
        // Guard in case visualToLogic isn't assigned yet
        if (visualToLogic != null)
            visualToLogic.OnVisualWordSet += HandleSetVisualWord;
        if (wordMarkup != null)
            wordMarkup.OnBietVaySpriteChange += RequestTypeChange;
        if (fadeWord != null)
            fadeWord.OnFadeWord += RequestTypeChange;
    }

    private void OnDisable()
    {
        if (visualToLogic != null)
            visualToLogic.OnVisualWordSet -= HandleSetVisualWord;
        if (wordMarkup != null)
        wordMarkup.OnBietVaySpriteChange -= RequestTypeChange;
        if (fadeWord != null)
            fadeWord.OnFadeWord -= RequestTypeChange;
        // unsubscribe worker if we had one
        UnwireWorker();
    }

    private void HandleSetVisualWord(GameObject visualWord)
    {
        // Unwire previous worker (if any)
        UnwireWorker();

        visualWordObject = visualWord;
        spriteColorChanger = visualWordObject != null
            ? visualWordObject.GetComponent<SpriteColorChanger>()
            : null;

        if (spriteColorChanger != null)
        {
            // Wire the worker to our instance event
            OnRequestTypeChange += spriteColorChanger.SetType;
        }
        else
        {
            Debug.LogWarning("Commander: SpriteColorChanger not found on visual word object.", this);
        }
    }

    private void UnwireWorker()
    {
        if (spriteColorChanger != null)
        {
            OnRequestTypeChange -= spriteColorChanger.SetType;
            spriteColorChanger = null;
        }
        visualWordObject = null;
    }

    // Call this from anywhere (UI button, input, etc.)
    public void RequestTypeChange(int id)
    {
        OnRequestTypeChange?.Invoke(id);
    }
}
using UnityEngine;
using UnityEngine.UI; // For RawImage

public class FinalActPsudo : MonoBehaviour
{
    public static bool ForceFinalActPriority = false; 

    private bool hasTriggered = false;
    public bool IsFinalActTriggered = false;

    [Header("Final Act Settings")]
    public WordPoolTrigger wordPoolTrigger;     // Drag the WordPoolTrigger here
    public WordsPackage finalActPackage;        // Drag the package you want to pour
    public RawImage targetRawImage;             // Drag the RawImage you want to clear

    private void OnEnable()
    {
        SentenceChecker.OnSentenceCorrect += OnSentenceCorrectHandler;
    }

    private void OnDisable()
    {
        SentenceChecker.OnSentenceCorrect -= OnSentenceCorrectHandler;
    }

    private void OnSentenceCorrectHandler()
    {
        // ✅ If forced priority, trigger FinalAct & skip normal SentenceChecker tween
        if (ForceFinalActPriority || IsFinalActTriggered)
        {
            Debug.Log("⚡ Final Act Priority → overriding SentenceChecker tween");
            TriggerFinalAct();
            return;
        }

        
    }

    public void FlagFinalAct()
    {
        IsFinalActTriggered = true;
        ForceFinalActPriority = true; 
    }

    public void TriggerFinalAct()
    {
        if (IsFinalActTriggered)
        {
            Debug.Log("🎬 Final Act Triggered → Pouring package, clearing RawImage, toggling PopOut.");

            // ✅ 1. Toggle off PopOut
            if (PopOutRectTween.Instance != null)
                PopOutRectTween.Instance.ToggleOff();

            // ✅ 2. Pour the final act word package
            if (wordPoolTrigger != null && finalActPackage != null)
            {
                wordPoolTrigger.LoadAndPour(finalActPackage);
            }
            else
            {
                Debug.LogWarning("⚠️ FinalAct: Missing WordPoolTrigger or finalActPackage!");
            }

            // ✅ 3. Clear the RawImage (remove texture)
            if (targetRawImage != null)
            {
                targetRawImage.texture = null;
                Debug.Log("🖼️ RawImage cleared!");
            }
            

        }

        hasTriggered = true;

    }
}
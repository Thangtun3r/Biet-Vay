using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class WordPackageProvider : MonoBehaviour
{
    public WordsPackage package;
    public TMP_Text     displayText;
    [TextArea] public string messageToType = "Your default prompt here...";
    public float        typewriterSpeed  = 0.05f;
    public float        bufferTime       = 1f;
    public bool isLastPackage = false; // If true, this is the last package to be displayed
    public FinalActPsudo finalActPsudo; // Reference to the FinalActPsudo script
    

    // ⮕ NEW event
    public event Action<WordPackageProvider> OnTypewriterComplete;

    public WordsPackage GetPackage(float customBuffer = -1f)
    {
        float waitTime = customBuffer > 0 ? customBuffer : bufferTime;
        if (displayText != null)
        {
            StopAllCoroutines();
            StartCoroutine(TypewriterRoutine(waitTime));
        }
        return package;
    }

    private IEnumerator TypewriterRoutine(float waitAfter)
    {
        displayText.text = "";
        displayText.gameObject.SetActive(true);
  

        foreach (char c in messageToType)
        {
            displayText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        yield return new WaitForSeconds(waitAfter);

        displayText.text = "";

        // ⮕ FIRE the completion event
        OnTypewriterComplete?.Invoke(this);
        TriggerFinalAct();
    }

    private void TriggerFinalAct()
    {
        if (isLastPackage)
        {
            finalActPsudo?.FlagFinalAct();
        }
    }
}
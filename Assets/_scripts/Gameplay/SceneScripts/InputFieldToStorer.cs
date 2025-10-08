using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputFieldToStorer : MonoBehaviour
{
    [Tooltip("Reference to the central SubmitStorer.")]
    public SubmitStorer storer;

    [Tooltip("Which field this input should update.")]
    public FieldType fieldType;

    [Tooltip("Optional: Reference to the Send button that triggers submission.")]
    public Button sendButton;

    // Support both legacy and TMP
    private InputField uiInput;
    private TMP_InputField tmpInput;

    public enum FieldType { Name, Email, BietVay, Overcome }

    void Awake()
    {
        uiInput  = GetComponent<InputField>();
        tmpInput = GetComponent<TMP_InputField>();

        // Update storer when user finishes typing
        if (uiInput)  uiInput.onEndEdit.AddListener(UpdateStorer);
        if (tmpInput) tmpInput.onEndEdit.AddListener(UpdateStorerTMP);

        // Ensure a final push happens right before submit
        if (sendButton) sendButton.onClick.AddListener(PushCurrentValue);
    }

    void UpdateStorer(string value)
    {
        if (!storer) return;
        switch (fieldType)
        {
            case FieldType.Name:     storer.nameValue     = value; break;
            case FieldType.Email:    storer.emailValue    = value; break;
            case FieldType.BietVay:  storer.bietVayValue  = value; break;
            case FieldType.Overcome: storer.overcomeValue = value; break;
        }
    }

    void UpdateStorerTMP(string value) => UpdateStorer(value);

    // Called on Send click to sync latest text even if field is still focused
    void PushCurrentValue()
    {
        if (!storer) return;
        if (tmpInput) { UpdateStorer(tmpInput.text); return; }
        if (uiInput)  { UpdateStorer(uiInput.text);  return; }
    }
}
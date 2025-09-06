// CharacterColorApplier.cs
// Attach to the same GameObject as LinePresenter.
// Assign 'database' OR place an asset named "CharacterColorDB" under a Resources/ folder to auto-load.

using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class CharacterColorApplier : MonoBehaviour
{
    [Header("Database (ScriptableObject)")]
    public CharacterColorDatabase database; // drag your asset here (or auto-loads)

    [Header("UI Target")]
    public Image nameDisplayer;             // e.g., speaker name BG/panel
    public Color fallbackColor = Color.white;

    public CustomLinePresenter presenter;

    private void Awake()
    {
        presenter = GetComponent<CustomLinePresenter>();

        // Auto-load from Resources if nothing assigned
        if (database == null)
            database = Resources.Load<CharacterColorDatabase>("CharacterColorDB");

        CustomLinePresenter.OnLineDisplayed += HandleLineDisplayed;
    }

    private void OnDestroy()
    {
        CustomLinePresenter.OnLineDisplayed -= HandleLineDisplayed;
    }

    private void HandleLineDisplayed()
    {
        if (presenter == null || nameDisplayer == null) return;

        // Read speaker name from LinePresenter
        var speaker = presenter.characterNameText != null ? presenter.characterNameText.text : string.Empty;

        if (database != null && database.TryGet(speaker, out var color))
            nameDisplayer.color = color;
        else
            nameDisplayer.color = fallbackColor;
    }
}

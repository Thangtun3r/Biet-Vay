// CharacterColorApplier.cs
// Attach to the same GameObject as CustomLinePresenter.
// Assign 'database' or place an asset named "CharacterColorDB" under a Resources/ folder.
// This version defers one frame so it reads the *current* speaker name after the presenter updates it.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

[DisallowMultipleComponent]
public class CharacterColorApplier : MonoBehaviour
{
    [Header("Database (ScriptableObject)")]
    [Tooltip("Drag your CharacterColorDatabase asset here. If left empty, will try Resources/CharacterColorDB")]
    public CharacterColorDatabase database;

    [Header("UI Target")]
    [Tooltip("UI Image to tint for the speaker (e.g., name BG/panel).")]
    public Image nameDisplayer;

    [Header("Defaults")]
    public Color fallbackColor = Color.white;

    private CustomLinePresenter presenter;

    // Small cache to avoid repeated lookups/allocations
    private readonly Dictionary<string, Color> _cache = new Dictionary<string, Color>();

    private void Awake()
    {
        presenter = GetComponent<CustomLinePresenter>();

        // Auto-load from Resources if nothing assigned
        if (database == null)
            database = Resources.Load<CharacterColorDatabase>("CharacterColorDB");
    }

    private void OnEnable()
    {
        CustomLinePresenter.OnLineDisplayed += HandleLineDisplayed;
    }

    private void OnDisable()
    {
        CustomLinePresenter.OnLineDisplayed -= HandleLineDisplayed;
    }

    private void HandleLineDisplayed()
    {
        // Defer to after the presenter has updated characterNameText
        // Using end-of-frame ensures TMP/layout updates have also settled.
        if (isActiveAndEnabled)
            StartCoroutine(ApplyColorAfterPresenter());
    }

    private IEnumerator ApplyColorAfterPresenter()
    {
        // Wait one frame (or end-of-frame) so CustomLinePresenter can set the name.
        yield return new WaitForEndOfFrame();

        if (presenter == null || nameDisplayer == null)
            yield break;

        var nameField = presenter.characterNameText;
        var rawName = nameField != null ? nameField.text : string.Empty;

        // Sanitize: trim whitespace, collapse doubles, and unify case
        var speaker = SanitizeName(rawName);

        // Nothing to do
        if (string.IsNullOrEmpty(speaker))
        {
            nameDisplayer.color = fallbackColor;
            yield break;
        }

        // Cache hit?
        if (_cache.TryGetValue(speaker, out var cached))
        {
            nameDisplayer.color = cached;
            yield break;
        }

        // Try database (case-insensitive). Your CharacterColorDatabase should ideally handle this;
        // we’ll do a light wrapper here using TryGet if available.
        Color resolved;
        if (database != null && database.TryGet(speaker, out resolved))
        {
            nameDisplayer.color = resolved;
            _cache[speaker] = resolved;
            yield break;
        }

        // If DB lookup failed, try a looser match (optional: useful if DB keys differ in case/spacing)
        if (database != null && database.TryGet(Loosen(speaker), out resolved))
        {
            nameDisplayer.color = resolved;
            _cache[speaker] = resolved;
            yield break;
        }

        // Fallback
        nameDisplayer.color = fallbackColor;
        _cache[speaker] = fallbackColor;
    }

    private static string SanitizeName(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        s = s.Trim();
        // Remove accidental newlines/tabs and extra spaces
        s = s.Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ');
        while (s.Contains("  ")) s = s.Replace("  ", " ");
        return s;
    }

    // A simple loosening step (lowercase); you can expand to strip punctuation if your DB keys vary
    private static string Loosen(string s) => s.ToLowerInvariant();
}

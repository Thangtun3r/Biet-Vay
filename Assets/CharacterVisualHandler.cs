// Attach this to the same GameObject as your LinePresenter
// and assign your references in the Inspector

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Yarn.Unity;

public class CharacterColorApplier : MonoBehaviour
{
    [System.Serializable]
    public class CharacterProfile
    {
        public string characterName;
        [Tooltip("Remember to use the format #RRGGBB or #AARRGGBB")]
        public string hexColor = "#FFFFFF";
    }

    [Header("Profile Settings")]
    public CharacterProfile[] profiles;

    [Header("UI Target")]
    public Image nameDisplayer; // e.g. background for speaker or portrait panel

    private Dictionary<string, Color> colorLookup;
    private LinePresenter presenter;

    private void Awake()
    {
        presenter = GetComponent<LinePresenter>();

        // Build color dictionary
        colorLookup = new Dictionary<string, Color>();
        foreach (var profile in profiles)
        {
            if (ColorUtility.TryParseHtmlString(profile.hexColor, out var color))
                colorLookup[profile.characterName] = color;
            else
                Debug.LogWarning($"Invalid hex color for {profile.characterName}: {profile.hexColor}");
        }

        LinePresenter.OnLineDisplayed += HandleLineDisplayed;
    }

    private void OnDestroy()
    {
        LinePresenter.OnLineDisplayed -= HandleLineDisplayed;
    }

    private void HandleLineDisplayed()
    {
        if (presenter == null || presenter.lineText == null)
            return;

        string currentSpeaker = presenter.characterNameText != null 
            ? presenter.characterNameText.text 
            : string.Empty;

        if (string.IsNullOrEmpty(currentSpeaker))
            return;

        if (colorLookup.TryGetValue(currentSpeaker, out var color))
        {
            if (nameDisplayer != null)
                nameDisplayer.color = color;
        }
    }
}
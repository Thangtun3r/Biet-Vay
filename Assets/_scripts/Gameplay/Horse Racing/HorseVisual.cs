using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class HorseVisual : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Optional: explicit reference to the horse logic. If null, will search in parents.")]
    public Horse2D horse;

    [Header("UI Visual")]
    public Image uiImage;

    [Header("Name UI")]
    public TextMeshProUGUI nameTMP;

    [Header("Result Display")]
    public GameObject resultPanel;
    public TextMeshProUGUI positionTMP;
    public TextMeshProUGUI timeTMP;

    [Header("Colors")]
    [Tooltip("Color used for normal (non-first) results.")]
    public Color regularColor = Color.white;
    [Tooltip("Color used when this horse finishes 1st.")]
    public Color goldColor = new Color(1f, 0.84f, 0f); // nice gold

    private bool _shown;

    void Awake()
    {
        if (horse == null) horse = GetComponentInParent<Horse2D>();
        if (uiImage == null) uiImage = GetComponentInChildren<Image>();
        if (nameTMP == null) nameTMP = GetComponentInChildren<TextMeshProUGUI>();
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    void Update()
    {
        if (horse == null) return;

        if (!_shown && horse.progress01 >= 1f)
        {
            ShowResult(horse.progress01);
            _shown = true;
        }
    }

    public void ApplyIdentity(string displayName, Color color, bool renameGameObject = false)
    {
        ApplyDisplayName(displayName, renameGameObject);
        ApplyColor(color);
    }

    public void ApplyDisplayName(string displayName, bool renameGameObject = false)
    {
        string safe = string.IsNullOrWhiteSpace(displayName) ? "" : displayName.Trim();
        if (nameTMP != null) nameTMP.text = safe;
        if (renameGameObject && safe.Length > 0) gameObject.name = safe;
    }

    public void ApplyColor(Color c)
    {
        if (uiImage != null) uiImage.color = c;
    }

    public void ShowResult(float timeSeconds, int place = -1)
    {
        Color chosenColor = (place == 1) ? goldColor : regularColor;

        if (positionTMP != null && place > 0)
        {
            positionTMP.text = place.ToString("0");
            positionTMP.color = chosenColor;
        }

        if (timeTMP != null)
        {
            timeTMP.text = FormatTime(timeSeconds);
            timeTMP.color = chosenColor;
        }

        if (nameTMP != null)
            nameTMP.color = chosenColor;

        if (resultPanel != null)
            resultPanel.SetActive(true);
    }

    public void HideResult()
    {
        if (resultPanel != null) resultPanel.SetActive(false);

        if (positionTMP != null)
        {
            positionTMP.text = "";
            positionTMP.color = regularColor;
        }

        if (timeTMP != null)
        {
            timeTMP.text = "";
            timeTMP.color = regularColor;
        }

        if (nameTMP != null)
            nameTMP.color = regularColor;

        _shown = false;
    }

    private static string FormatTime(float seconds)
    {
        if (seconds < 0f) seconds = 0f;
        int minutes = Mathf.FloorToInt(seconds / 60f);
        float sec = seconds % 60f;
        return $"{minutes}:{sec:00.00}";
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;
using DG.Tweening;
using System.Collections;

public class GashaponRoller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private Image rarityBackground;
    [SerializeField] private TextMeshProUGUI rarityLabel;

    [Header("Config")]
    [Range(0f, 1f)] [SerializeField] private float specialChance = 0.10f;
    [SerializeField] private string yarnBoolVariable = "$favoriteGashapon";

    [Header("Colors")]
    [SerializeField] private Color specialBg = new Color(1f, 0.9f, 0.1f); // yellow
    [SerializeField] private Color normalBg  = Color.grey;                 // grey
    [SerializeField] private Color textColor = Color.white;                // white

    [Header("Animation")]
    [SerializeField] private float anticipationScale = 1.2f;
    [SerializeField] private float anticipationDuration = 0.3f;
    [SerializeField] private float revealDuration = 0.4f;

    private void Awake()
    {
        if (dialogueRunner == null)
            dialogueRunner = FindObjectOfType<DialogueRunner>();

        if (dialogueRunner == null)
        {
            Debug.LogError("[GashaponRoller] DialogueRunner reference is missing.");
            return;
        }

        // Keep your Yarn commands
        dialogueRunner.AddCommandHandler("roll", RollCoroutine);
        dialogueRunner.AddCommandHandler<float>("roll_chance", RollWithChanceCoroutine);

        SetUIActive(false);
    }

    private void OnEnable()
    {
        // ONLY subscribe to VMReset
        GameManager.OnVMReset += HandleVMReset;
    }

    private void OnDisable()
    {
        GameManager.OnVMReset -= HandleVMReset;
        KillTweens();
    }

    // === Yarn: <<roll>> ===
    private IEnumerator RollCoroutine()
    {
        bool isSpecial = Random.value < Mathf.Clamp01(specialChance);
        yield return PlayRollAnimation(isSpecial);
    }

    // === Yarn: <<roll_chance 0.25>> ===
    private IEnumerator RollWithChanceCoroutine(float chance)
    {
        specialChance = Mathf.Clamp01(chance);
        yield return RollCoroutine();
    }

    private IEnumerator PlayRollAnimation(bool isSpecial)
    {
        if (rarityBackground == null || rarityLabel == null)
        {
            Debug.LogWarning("[GashaponRoller] UI references missing.");
            yield break;
        }

        KillTweens();

        // Show + reset
        SetUIActive(true);
        rarityBackground.color = Color.clear;
        rarityLabel.text = "";
        rarityLabel.color = textColor;

        // Anticipation bump
        transform.localScale = Vector3.one;
        yield return transform
            .DOScale(anticipationScale, anticipationDuration)
            .SetEase(Ease.OutBack)
            .WaitForCompletion();

        // Reveal + push result to Yarn
        ApplyVisual(isSpecial);
        SetYarnBool(isSpecial);

        // Ease back
        yield return transform
            .DOScale(1f, revealDuration)
            .SetEase(Ease.InOutQuad)
            .WaitForCompletion();
    }

    private void ApplyVisual(bool isSpecial)
    {
        rarityBackground.color = isSpecial ? specialBg : normalBg;
        rarityLabel.text = isSpecial ? "SPECIAL!" : "Normal";
        rarityLabel.color = textColor;
    }

    private void SetYarnBool(bool value)
    {
        if (dialogueRunner == null || string.IsNullOrEmpty(yarnBoolVariable))
            return;

        dialogueRunner.VariableStorage.SetValue(yarnBoolVariable, value);
    }

    private void SetUIActive(bool state)
    {
        if (rarityBackground != null) rarityBackground.gameObject.SetActive(state);
        if (rarityLabel != null)      rarityLabel.gameObject.SetActive(state);
    }

    private void KillTweens()
    {
        if (transform != null) transform.DOKill(false);
    }

    // === ONLY reaction to Yarn <<VMReset>> via GameManager ===
    private void HandleVMReset()
    {
        KillTweens();
        SetUIActive(false); // hides image + text, as you wanted
        // optional: clear the Yarn flag if you prefer a fresh state next time:
        // SetYarnBool(false);
    }
}

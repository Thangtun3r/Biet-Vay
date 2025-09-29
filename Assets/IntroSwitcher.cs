using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpriteSwitcher : MonoBehaviour
{
    [Header("Yarn nodes")]
    public string yarnLastNodeName;   // called when we land on the last sprite
    public string yarnIntroNodeName;  // called ONLY when we move back from last to a non-last sprite

    [Header("Assign UI Image and Sprites")]
    public Image targetImage;
    public Sprite[] sprites;

    [Header("Animator")]
    public Animator animator;     // has "NextFrame" trigger

    [Header("Buttons")]
    public Button forwardButton;  // click → go forward
    public Button backButton;     // click → go backward

    [Header("Settings")]
    public float delayBeforeChange = 0.2f; // delay before sprite updates
    public float inputCooldown = 0.5f;     // cooldown AFTER sprite change

    private int currentIndex = 0;
    private Coroutine changeRoutine;
    private float nextAllowedTime = 0f;
    private bool isProcessing = false;

    int LastIndex => Mathf.Max(0, sprites.Length - 1);

    void OnEnable()
    {
        // subscribe to external shutdown event
        GameManager.OnPsudoTurnOff += HandlePsudoTurnOff;
    }

    void OnDisable()
    {
        // unsubscribe to avoid leaks / duplicate handlers
        GameManager.OnPsudoTurnOff -= HandlePsudoTurnOff;
    }

    void Start()
    {
        if (sprites.Length > 0 && targetImage != null)
            targetImage.sprite = sprites[currentIndex];

        if (forwardButton) forwardButton.onClick.AddListener(() => TryStartChange(+1));
        if (backButton)    backButton.onClick.AddListener(() => TryStartChange(-1));

        UpdateButtonStates();
    }

    void TryStartChange(int dir)
    {
        if (!enabled) return; // safety if invoked by UI while disabled
        if (sprites.Length == 0 || targetImage == null) return;
        if (Time.time < nextAllowedTime) return; // cooldown active
        if (isProcessing) return;                // waiting for delayed change

        int newIndex = Mathf.Clamp(currentIndex + dir, 0, LastIndex);
        if (newIndex == currentIndex) return;

        // detect if we're going BACK from the LAST sprite
        bool fromLastToNotLast = (currentIndex == LastIndex) && (newIndex < LastIndex);

        if (animator) animator.SetTrigger("NextFrame");
        changeRoutine = StartCoroutine(ChangeSpriteAfterDelay(newIndex, fromLastToNotLast));
    }

    // accept a flag telling us if we moved back from last
    IEnumerator ChangeSpriteAfterDelay(int newIndex, bool fromLastToNotLast)
    {
        isProcessing = true;

        yield return new WaitForSeconds(delayBeforeChange);

        currentIndex = newIndex;
        targetImage.sprite = sprites[currentIndex];

        // ---- Yarn triggers ----
        if (currentIndex == LastIndex)
        {
            // Landed on the LAST sprite
            if (!string.IsNullOrEmpty(yarnLastNodeName))
                YarnDialogueEventBridge.CallYarnEvent(yarnLastNodeName);
        }
        else if (fromLastToNotLast)
        {
            // ONLY when moving back off the last sprite
            yield return new WaitForSeconds(0.1f); // tiny buffer, optional
            if (!string.IsNullOrEmpty(yarnIntroNodeName))
                YarnDialogueEventBridge.CallYarnEvent(yarnIntroNodeName);
        }

        nextAllowedTime = Time.time + inputCooldown; // cooldown starts after change
        isProcessing = false;
        changeRoutine = null;

        UpdateButtonStates();
    }

    void UpdateButtonStates()
    {
        if (forwardButton)
            forwardButton.gameObject.SetActive(currentIndex < LastIndex); // hide when on last

        if (backButton)
            backButton.gameObject.SetActive(currentIndex > 0);            // hide when on first
    }

    // --- NEW: external control ---

    void HandlePsudoTurnOff()
    {
        DisableSwitcher();
    }

    /// <summary>
    /// Cleanly disables this switcher:
    /// - Stops active coroutine
    /// - Hides navigation buttons so UI can’t trigger it
    /// - Disables this component
    /// </summary>
    public void DisableSwitcher()
    {
        if (changeRoutine != null)
        {
            StopCoroutine(changeRoutine);
            changeRoutine = null;
        }

        isProcessing = false;
        nextAllowedTime = float.PositiveInfinity;

        if (forwardButton) forwardButton.gameObject.SetActive(false);
        if (backButton)    backButton.gameObject.SetActive(false);

        enabled = false;
    }
}

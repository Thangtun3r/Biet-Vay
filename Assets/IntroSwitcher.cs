using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpriteSwitcherSimple : MonoBehaviour
{
    [Header("UI")]
    public Image targetImage;            // single image to display the sprite

    [Header("Sprites in order")]
    public Sprite[] sprites;

    [Header("Animator")]
    public Animator animator;            // uses ONE trigger for any transition
    public string transitionTrigger = "Transition";

    [Header("Buttons (optional)")]
    public Button forwardButton;         // cycles +1
    public Button backButton;            // cycles -1 (still uses the same trigger)

    [Header("Timing")]
    [Tooltip("Wait time that matches your transition animation length before swapping the sprite.")]
    [SerializeField] private float commitDelay = 0.25f;
    [SerializeField] private float inputCooldown = 0.25f;

    [Header("Yarn")]
    public string lastNodeName = "LastNode";
    public string previousNodeName = "PreviousNode";

    [Header("Safety")]
    [Tooltip("If other systems re-enable the buttons, enforce the correct visibility every frame.")]
    [SerializeField] private bool enforceVisibilityEveryFrame = true;

    private int currentIndex = 0;
    private bool isProcessing = false;
    private float nextAllowedTime = 0f;

    int LastIndex => Mathf.Max(0, sprites.Length - 1);
    int NextIndex(int i) => Mathf.Min(i + 1, LastIndex);
    int PrevIndex(int i) => Mathf.Max(i - 1, 0);

    void Start()
    {
        if (!targetImage || sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning("SpriteSwitcherSimple: assign targetImage and at least 1 sprite.");
            enabled = false; 
            return;
        }

        // initial load
        currentIndex = Mathf.Clamp(currentIndex, 0, LastIndex);
        targetImage.sprite = sprites[currentIndex];

        if (forwardButton) forwardButton.onClick.AddListener(Next);
        if (backButton)    backButton.onClick.AddListener(Back);

        UpdateButtonStates();
    }

    void OnEnable()
    {
        // In case a parent Canvas/Panel was re-enabled by another system
        UpdateButtonStates();
    }

    void LateUpdate()
    {
        if (!enforceVisibilityEveryFrame) return;

        // Continuously enforce visibility so no other system can flip it
        if (forwardButton)
            forwardButton.gameObject.SetActive(currentIndex < LastIndex);

        if (backButton)
            backButton.gameObject.SetActive(currentIndex > 0);
    }

    public void Next()
    {
        TryStartChange(NextIndex(currentIndex), triggerPrevNodeIfBackingFromLast: false);
    }

    public void Back()
    {
        // Only trigger the "previous node" if we are backing *from the last sprite*
        bool shouldTriggerPrev = (currentIndex == LastIndex);
        TryStartChange(PrevIndex(currentIndex), triggerPrevNodeIfBackingFromLast: shouldTriggerPrev);
    }

    public void GoTo(int index)
    {
        TryStartChange(Mathf.Clamp(index, 0, LastIndex), triggerPrevNodeIfBackingFromLast: false);
    }

    void TryStartChange(int targetIndex, bool triggerPrevNodeIfBackingFromLast)
    {
        if (Time.time < nextAllowedTime || isProcessing) return;
        if (targetIndex == currentIndex) return;

        StartCoroutine(PlayTransitionAndSwap(targetIndex, triggerPrevNodeIfBackingFromLast));
    }

    IEnumerator PlayTransitionAndSwap(int targetIndex, bool triggerPrevNodeIfBackingFromLast)
    {
        isProcessing = true;

        // Kick the single transition animation
        if (animator && !string.IsNullOrEmpty(transitionTrigger))
            animator.SetTrigger(transitionTrigger);

        // Wait for transition before swapping
        if (commitDelay > 0f) yield return new WaitForSeconds(commitDelay);

        currentIndex = targetIndex;
        targetImage.sprite = sprites[currentIndex];

        // --- Yarn hooks ---
        if (currentIndex == LastIndex && !string.IsNullOrEmpty(lastNodeName))
            CallYarnEventSafe(lastNodeName);

        if (triggerPrevNodeIfBackingFromLast && !string.IsNullOrEmpty(previousNodeName))
            CallYarnEventSafe(previousNodeName);
        // ------------------

        nextAllowedTime = Time.time + inputCooldown;
        isProcessing = false;

        UpdateButtonStates();
    }

    void UpdateButtonStates()
    {
        // Hide next button on last sprite
        if (forwardButton)
            forwardButton.gameObject.SetActive(currentIndex < LastIndex);

        // Hide back button on first sprite
        if (backButton)
            backButton.gameObject.SetActive(currentIndex > 0);
    }

    void CallYarnEventSafe(string nodeName)
    {
        // Expecting a static method with this exact signature on your bridge
        YarnDialogueEventBridge.CallYarnEvent(nodeName);
    }
}

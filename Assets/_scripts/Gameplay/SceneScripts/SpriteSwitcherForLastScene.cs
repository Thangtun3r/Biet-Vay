using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class Step
{
    [Header("Visual")]
    public Sprite sprite;

    [Header("Yarn")]
    [Tooltip("Yarn node to call after this step becomes active.")]
    public string yarnNodeName;

    [Header("Input gating")]
    [Tooltip("If true, Forward is disabled and Send button must be pressed to advance.")]
    public bool input = false;

    [Tooltip("Optional: assign the GameObject (like your Input Field container) for this step.")]
    public GameObject inputFieldObject;
}

public class SpriteSwitcherForLastScene : MonoBehaviour
{
    [Header("UI")]
    public Image targetImage;

    [Header("Steps (ordered)")]
    public Step[] steps;

    [Header("Animator")]
    public Animator animator;
    public string transitionTrigger = "Transition";

    [Header("Buttons")]
    public Button forwardButton;
    public Button backButton;
    public Button sendButton;

    [Header("Timing")]
    [Tooltip("Wait time that matches your transition animation before swapping the sprite.")]
    [SerializeField] private float commitDelay = 0.25f;
    [SerializeField] private float inputCooldown = 0.25f;

    [Header("Input Delay")]
    [Tooltip("Delay before enabling the input field object when entering an input step from a non-input step.")]
    [SerializeField] private float inputEnableDelay = 0.35f;

    [Header("Behavior")]
    [Tooltip("Invoke the Yarn node for the starting step on Start().")]
    [SerializeField] private bool callNodeOnInitialLoad = false;

    [Header("Safety")]
    [Tooltip("If other systems re-enable the buttons, enforce the correct visibility every frame.")]
    [SerializeField] private bool enforceVisibilityEveryFrame = true;

    private int currentIndex = 0;
    private bool isProcessing = false;
    private float nextAllowedTime = 0f;

    int LastIndex => Mathf.Max(0, (steps?.Length ?? 0) - 1);
    int NextIndex(int i) => Mathf.Min(i + 1, LastIndex);
    int PrevIndex(int i) => Mathf.Max(i - 1, 0);

    void Start()
    {
        if (!targetImage || steps == null || steps.Length == 0)
        {
            Debug.LogWarning("SpriteSwitcherForLastScene: assign targetImage and at least 1 Step.");
            enabled = false;
            return;
        }

        // Button wiring
        if (forwardButton) forwardButton.onClick.AddListener(Next);
        if (backButton) backButton.onClick.AddListener(Back);
        if (sendButton) sendButton.onClick.AddListener(Next);

        // Initial setup
        currentIndex = Mathf.Clamp(currentIndex, 0, LastIndex);
        ApplySprite(currentIndex);
        HideAllInputObjects();
        SafeSetActive(steps[currentIndex].inputFieldObject, steps[currentIndex].input);

        if (callNodeOnInitialLoad)
            CallYarnForIndex(currentIndex);

        UpdateButtonStates();
    }

    void OnEnable() => UpdateButtonStates();

    void LateUpdate()
    {
        if (!enforceVisibilityEveryFrame) return;
        UpdateButtonStates();
    }

    public void Next() => TryStartChange(NextIndex(currentIndex));
    public void Back() => TryStartChange(PrevIndex(currentIndex));
    public void GoTo(int index) => TryStartChange(Mathf.Clamp(index, 0, LastIndex));

    void TryStartChange(int targetIndex)
    {
        if (Time.time < nextAllowedTime || isProcessing) return;
        if (targetIndex == currentIndex) return;

        StartCoroutine(PlayTransitionAndSwap(targetIndex));
    }

    IEnumerator PlayTransitionAndSwap(int targetIndex)
    {
        isProcessing = true;

        // Play transition animation
        if (animator && !string.IsNullOrEmpty(transitionTrigger))
            animator.SetTrigger(transitionTrigger);

        // Wait for sprite swap timing
        if (commitDelay > 0f)
            yield return new WaitForSeconds(commitDelay);

        int prevIndex = currentIndex;
        var prevStep = steps[prevIndex];

        currentIndex = targetIndex;
        var currStep = steps[currentIndex];

        // Change sprite and call Yarn immediately
        ApplySprite(currentIndex);
        CallYarnForIndex(currentIndex);

        // Handle input object activation
        HideAllInputObjects();

        if (!prevStep.input && currStep.input && currStep.inputFieldObject != null)
        {
            // Delay enabling input object
            StartCoroutine(EnableInputAfterDelay(currStep.inputFieldObject, inputEnableDelay));
        }
        else
        {
            SafeSetActive(currStep.inputFieldObject, currStep.input);
        }

        nextAllowedTime = Time.time + inputCooldown;
        isProcessing = false;

        UpdateButtonStates();
    }

    void ApplySprite(int i)
    {
        if (i < 0 || i >= steps.Length) return;
        targetImage.sprite = steps[i].sprite;
    }

    void HideAllInputObjects()
    {
        if (steps == null) return;
        foreach (var step in steps)
        {
            SafeSetActive(step.inputFieldObject, false);
        }
    }

    IEnumerator EnableInputAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        SafeSetActive(obj, true);
    }

    void SafeSetActive(GameObject obj, bool active)
    {
        if (obj && obj.activeSelf != active)
            obj.SetActive(active);
    }

    void UpdateButtonStates()
    {
        if (steps == null || steps.Length == 0) return;

        bool isFirst = currentIndex <= 0;
        bool isLast = currentIndex >= LastIndex;
        bool needsInput = steps[currentIndex].input;

        if (forwardButton)
        {
            forwardButton.gameObject.SetActive(!isLast && !needsInput);
            forwardButton.interactable = !isProcessing && !needsInput;
        }

        if (sendButton)
        {
            sendButton.gameObject.SetActive(needsInput);
            sendButton.interactable = !isProcessing;
        }

        if (backButton)
        {
            backButton.gameObject.SetActive(!isFirst);
            backButton.interactable = !isProcessing;
        }
    }

    void CallYarnForIndex(int i)
    {
        if (i < 0 || i >= steps.Length) return;
        string nodeName = steps[i].yarnNodeName;
        if (string.IsNullOrEmpty(nodeName)) return;

        YarnDialogueEventBridge.CallYarnEvent(nodeName);
    }
}

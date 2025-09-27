using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpriteSwitcher : MonoBehaviour
{
    [Header("Yarn nodes")]
    public string yarnLastNodeName;   // called when we land on the last sprite
    public string yarnIntroNodeName;  // called when we land on any non-last sprite

    [Header("Assign UI Image and Sprites")]
    public Image targetImage;
    public Sprite[] sprites;

    [Header("Animator")]
    public Animator animator;     // has "NextFrame" trigger

    [Header("Settings")]
    public float delayBeforeChange = 0.2f; // delay before sprite updates
    public float inputCooldown = 0.5f;     // cooldown AFTER sprite change

    private int currentIndex = 0;
    private Coroutine changeRoutine;
    private float nextAllowedTime = 0f;
    private bool isProcessing = false;

    int LastIndex => Mathf.Max(0, sprites.Length - 1);

    void Start()
    {
        if (sprites.Length > 0 && targetImage != null)
            targetImage.sprite = sprites[currentIndex];
    }

    void Update()
    {
        if (sprites.Length == 0 || targetImage == null) return;
        if (Time.time < nextAllowedTime) return; // cooldown active
        if (isProcessing) return;                // waiting for delayed change

        if (Input.GetKeyDown(KeyCode.S))       // forward (no loop)
            TryStartChange(+1);
        else if (Input.GetKeyDown(KeyCode.W))  // backward (no loop)
            TryStartChange(-1);
    }

    void TryStartChange(int dir)
    {
        // No looping: clamp to [0, LastIndex]
        int newIndex = Mathf.Clamp(currentIndex + dir, 0, LastIndex);

        // If at an end already, do nothing (don’t trigger animator/Yarn)
        if (newIndex == currentIndex) return;

        if (animator) animator.SetTrigger("NextFrame");
        changeRoutine = StartCoroutine(ChangeSpriteAfterDelay(newIndex));
    }

    IEnumerator ChangeSpriteAfterDelay(int newIndex)
    {
        isProcessing = true;
    
        yield return new WaitForSeconds(delayBeforeChange);
    
        currentIndex = newIndex;
        targetImage.sprite = sprites[currentIndex];
    
        // ---- Yarn triggers ----
        if (currentIndex == LastIndex)
        {
            // We're on the LAST sprite -> fire last-frame Yarn immediately
            if (!string.IsNullOrEmpty(yarnLastNodeName))
                YarnDialogueEventBridge.CallYarnEvent(yarnLastNodeName);
        }
        else
        {
            // add a tiny buffer before firing the intro node
            yield return new WaitForSeconds(0.1f);
    
            if (!string.IsNullOrEmpty(yarnIntroNodeName))
                YarnDialogueEventBridge.CallYarnEvent(yarnIntroNodeName);
        }
    
        nextAllowedTime = Time.time + inputCooldown; // cooldown starts after change
        isProcessing = false;
        changeRoutine = null;
    }

}

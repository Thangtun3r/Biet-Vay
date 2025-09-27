using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpriteSwitcher : MonoBehaviour
{
    [Header("Assign UI Image and Sprites")]
    public Image targetImage;     // The UI Image to change
    public Sprite[] sprites;      // Array of sprites to cycle through

    [Header("Animator")]
    public Animator animator;     // Animator with "NextFrame" trigger

    [Header("Settings")]
    public float delayBeforeChange = 0.2f; // Delay in seconds before sprite changes

    private int currentIndex = 0;
    private Coroutine changeRoutine;

    void Start()
    {
        if (sprites.Length > 0 && targetImage != null)
        {
            targetImage.sprite = sprites[currentIndex];
        }
    }

    void Update()
    {
        if (sprites.Length == 0 || targetImage == null) return;

        if (Input.GetKeyDown(KeyCode.S)) // Next
        {
            int nextIndex = (currentIndex + 1) % sprites.Length;
            StartChange(nextIndex);
        }
        else if (Input.GetKeyDown(KeyCode.W)) // Back
        {
            int nextIndex = (currentIndex - 1 + sprites.Length) % sprites.Length;
            StartChange(nextIndex);
        }
    }

    void StartChange(int newIndex)
    {
        // Stop any running coroutine so only the latest input counts
        if (changeRoutine != null) StopCoroutine(changeRoutine);

        // Trigger animator immediately
        if (animator != null)
        {
            animator.SetTrigger("NextFrame");
        }

        // Start delayed sprite change
        changeRoutine = StartCoroutine(ChangeSpriteAfterDelay(newIndex));
    }

    IEnumerator ChangeSpriteAfterDelay(int newIndex)
    {
        yield return new WaitForSeconds(delayBeforeChange);

        currentIndex = newIndex;
        targetImage.sprite = sprites[currentIndex];
        changeRoutine = null;
    }
}
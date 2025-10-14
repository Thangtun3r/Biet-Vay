using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ErrorWordVisualIndicator : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image targetImage;         // The image to switch
    [SerializeField] private Sprite defaultSprite;      // Normal sprite
    [SerializeField] private Sprite errorSprite;        // Error sprite

    [Header("Settings")]
    [SerializeField] private float errorDuration = 0.3f; // Seconds to show error sprite

    private Coroutine activeRoutine;


    private void OnEnable()
    {
        SentenceChecker.OnCheckError += TriggerError;
    }
    private void OnDisable()
    {
        SentenceChecker.OnCheckError -= TriggerError;
    }

    /// <summary>
    /// Call this method to trigger the error visual effect.
    /// </summary>
    public void TriggerError()
    {
        // Prevent overlapping coroutines
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(ShowErrorSprite());
    }

    private IEnumerator ShowErrorSprite()
    {
        if (targetImage == null || errorSprite == null || defaultSprite == null)
            yield break;

        targetImage.sprite = errorSprite;
        yield return new WaitForSeconds(errorDuration);
        targetImage.sprite = defaultSprite;

        activeRoutine = null;
    }

    // Optional: auto-assign the Image if on the same GameObject
    private void Reset()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }
}
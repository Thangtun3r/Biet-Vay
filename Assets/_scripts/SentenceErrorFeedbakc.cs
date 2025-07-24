using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SentenceErrorFeedback : MonoBehaviour
{
    [Header("Image Flash Settings")]
    [Tooltip("The Image to flash")]
    public Image targetImage;
    [Tooltip("Color to flash on error")]
    public Color errorColor = Color.red;
    [Tooltip("Flash duration (seconds) each way)")]
    public float flashDuration = 0.15f;

    [Header("Shake Settings")]
    [Tooltip("Duration of shake (seconds)")]
    public float shakeDuration = 0.5f;
    [Tooltip("Strength of shake in world units")]
    public float shakeStrength = 0.5f;
    [Tooltip("Vibrato (how many shakes)")]
    public int   shakeVibrato  = 10;

    private Color originalColor;

    private void Awake()
    {
        if (targetImage != null)
            originalColor = targetImage.color;
    }

    private void OnEnable()
    {
        SentenceChecker.OnSentenceWrong += PlayErrorFeedback;
    }

    private void OnDisable()
    {
        SentenceChecker.OnSentenceWrong -= PlayErrorFeedback;
    }

    private void PlayErrorFeedback()
    {
        // 1) Flash color: original → error → original
        if (targetImage != null)
        {
            targetImage
                .DOColor(errorColor, flashDuration)
                .OnComplete(() => targetImage.DOColor(originalColor, flashDuration));
        }

        // 2) Shake position
        transform
            .DOShakePosition(shakeDuration, shakeStrength, shakeVibrato)
            .SetLink(gameObject); // auto-kill on destroy
    }
}


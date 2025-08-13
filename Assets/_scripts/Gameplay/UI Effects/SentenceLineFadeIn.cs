using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SentenceLineFadeIn : MonoBehaviour
{
    [Header("Fade Settings")]
    public Image[] sentenceLineFadeIn; // Array of Images
    public float fadeDuration = 1f;    // Duration for each fade-in

    private void OnEnable()
    {
        foreach (var img in sentenceLineFadeIn)
        {
            if (img != null)
            {
                // Start fully transparent
                Color c = img.color;
                c.a = 0f;
                img.color = c;

                StartCoroutine(FadeInImage(img, fadeDuration));
            }
        }
    }

    private IEnumerator FadeInImage(Image img, float duration)
    {
        float elapsed = 0f;
        Color c = img.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / duration);
            img.color = c;
            yield return null;
        }

        // Ensure fully visible at end
        c.a = 1f;
        img.color = c;
    }
}
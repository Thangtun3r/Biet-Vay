using UnityEngine;
using DG.Tweening;
using System.Collections;

public class RandomLightningFlash : MonoBehaviour
{
    [Header("Material Settings")]
    public Renderer targetRenderer;
    public float flashAlpha = 1f;   // Peak brightness
    public float normalAlpha = 0f;  // Resting darkness

    [Header("Flash Settings")]
    public float flashInDuration = 0.05f;  // How quickly lightning appears
    public float flashOutDuration = 0.3f;  // How slowly it fades
    public Vector2 intervalRange = new Vector2(2f, 5f); // Time between storms
    public Vector2 flashesPerStrike = new Vector2(1, 3); // Min/Max flickers per strike
    public float timeBetweenFlashes = 0.1f; // Delay between flickers in one strike

    private Material mat;

    void Start()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        mat = targetRenderer.material;

        // Ensure material starts at normal alpha
        Color c = mat.color;
        c.a = normalAlpha;
        mat.color = c;

        StartCoroutine(RandomFlashRoutine());
    }

    private IEnumerator RandomFlashRoutine()
    {
        while (true)
        {
            // Wait for a random interval before next strike
            yield return new WaitForSeconds(Random.Range(intervalRange.x, intervalRange.y));

            int flashCount = Random.Range((int)flashesPerStrike.x, (int)flashesPerStrike.y + 1);

            for (int i = 0; i < flashCount; i++)
            {
                // Flash up QUICKLY
                mat.DOFade(flashAlpha, flashInDuration);

                // Wait briefly, then fade out more slowly
                yield return new WaitForSeconds(flashInDuration);
                mat.DOFade(normalAlpha, flashOutDuration);

                // Small pause between flickers in the same strike
                yield return new WaitForSeconds(timeBetweenFlashes);
            }
        }
    }
}
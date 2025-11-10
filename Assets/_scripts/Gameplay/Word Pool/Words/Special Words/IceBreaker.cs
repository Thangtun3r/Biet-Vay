using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using FMODUnity; // ✅ Import FMODUnity namespace

public class IceBreaker : MonoBehaviour
{
    [Header("Sprites to cycle through")]
    public Sprite[] iceSprites;

    [Header("Particle System")]
    [Tooltip("ParticleSystem to play each time BreakIce() is called.")]
    public ParticleSystem breakParticles;

    [Header("FMOD")]
    [Tooltip("FMOD event that plays each time BreakIce() is called.")]
    public EventReference breakSound; // ✅ Drag your FMOD event here

    [Header("Clicks needed per stage (random)")]
    [Min(1)] public int minClicksToAdvance = 1;
    [Min(1)] public int maxClicksToAdvance = 3;

    private int currentIndex = 0;          // which sprite is currently shown
    private int clicksRemaining = 1;       // how many more BreakIce() calls until we advance
    private Image imageComponent;

    public static event Action OnIceBroken;

    void Awake()
    {
        imageComponent = GetComponent<Image>();

        if (imageComponent == null || iceSprites == null || iceSprites.Length == 0)
        {
            enabled = false; // nothing to do
            return;
        }

        // Start at the first sprite
        currentIndex = 0;
        imageComponent.sprite = iceSprites[0];

        // Start with a random click budget for advancing to sprite[1]
        clicksRemaining = RollClicks();
    }

    /// <summary>
    /// Plays particles, FMOD sound, and handles random-click progression.
    /// </summary>
    public void BreakIce()
    {
        if (!isActiveAndEnabled) return;

        // ✅ Play particle FX
        if (breakParticles != null)
            breakParticles.Play();

        // ✅ Play FMOD one-shot sound
        if (!breakSound.IsNull)
            RuntimeManager.PlayOneShot(breakSound, transform.position);

        // If we're already showing the last sprite, one more trigger disables.
        if (currentIndex >= iceSprites.Length - 1)
        {
            OnIceBroken?.Invoke();
            Debug.Log("Ice fully broken!");
            gameObject.SetActive(false);
            return;
        }

        // We're in an intermediate stage: consume a click.
        clicksRemaining--;
        if (clicksRemaining > 0) return;

        // Time to advance to the next sprite.
        currentIndex++;
        imageComponent.sprite = iceSprites[currentIndex];

        // If we just reached the last sprite, stop here (next call will disable).
        if (currentIndex >= iceSprites.Length - 1) return;

        // Otherwise, roll a new random click budget for the next advance.
        clicksRemaining = RollClicks();
    }

    private int RollClicks()
    {
        int lo = Mathf.Max(1, Mathf.Min(minClicksToAdvance, maxClicksToAdvance));
        int hi = Mathf.Max(lo, Mathf.Max(minClicksToAdvance, maxClicksToAdvance));
        return Random.Range(lo, hi + 1);
    }
}

using UnityEngine;
using FMODUnity;

public class FMODOneShotPlayer : MonoBehaviour
{
    [Header("FMOD Event")]
    [Tooltip("The FMOD event to play (e.g. event:/SFX/Explosion)")]
    [SerializeField] private EventReference fmodEvent;

    [Header("Settings")]
    [Tooltip("If true, play sound at this GameObject's position. Otherwise, plays 2D.")]
    [SerializeField] private bool use3DPosition = true;

    /// <summary>
    /// Plays the assigned FMOD one-shot event.
    /// </summary>
    public void PlayOneShot()
    {
        if (!fmodEvent.IsNull)
        {
            if (use3DPosition)
            {
                RuntimeManager.PlayOneShot(fmodEvent, transform.position);
            }
            else
            {
                RuntimeManager.PlayOneShot(fmodEvent);
            }
        }
        else
        {
            Debug.LogWarning($"[FMODOneShotPlayer] No FMOD event assigned on {gameObject.name}.");
        }
    }

    /// <summary>
    /// Optionally allows playing any FMOD event by reference.
    /// </summary>
    public void PlayOneShot(EventReference customEvent)
    {
        if (!customEvent.IsNull)
        {
            RuntimeManager.PlayOneShot(customEvent, transform.position);
        }
        else
        {
            Debug.LogWarning("[FMODOneShotPlayer] Tried to play a null event reference.");
        }
    }
}
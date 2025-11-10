using UnityEngine;
using FMODUnity;
using FMOD.Studio;

[RequireComponent(typeof(Collider))]
public class FMODStudioEmitterStay : MonoBehaviour
{
    [Tooltip("FMOD Event Reference")]
    public EventReference fmodEvent;

    private EventInstance eventInstance;
    private bool isPlaying = false;
    private Collider triggerCollider;
    private Collider playerCollider;

    private void Start()
    {
        triggerCollider = GetComponent<Collider>();
        if (!triggerCollider.isTrigger)
        {
            triggerCollider.isTrigger = true;
            Debug.LogWarning($"{name}: Collider was not a trigger. Setting isTrigger = true.");
        }

        // Cache the player’s collider (optional optimization)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player_Audio");
        if (playerObj != null)
            playerCollider = playerObj.GetComponent<Collider>();
    }

    private void Update()
    {
        if (playerCollider == null) return;

        bool isInside = triggerCollider.bounds.Intersects(playerCollider.bounds);

        // Player just entered
        if (isInside && !isPlaying)
        {
            if (!fmodEvent.IsNull)
            {
                eventInstance = RuntimeManager.CreateInstance(fmodEvent);
                eventInstance.start();
                Debug.Log($"{name}: FMOD event started (teleport or enter).");
                isPlaying = true;
            }
        }
        // Player just left
        else if (!isInside && isPlaying)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            eventInstance.release();
            eventInstance.clearHandle();
            isPlaying = false;
            Debug.Log($"{name}: FMOD event stopped (left or teleported out).");
        }
    }

    private void OnDestroy()
    {
        if (isPlaying && eventInstance.isValid())
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
    }
}

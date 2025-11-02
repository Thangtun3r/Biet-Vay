using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODRealtimeDebugger : MonoBehaviour
{
    // How often to update the log (seconds)
    [SerializeField] private float updateInterval = 1.0f;

    // Internal cache of currently playing emitters
    private readonly Dictionary<StudioEventEmitter, PLAYBACK_STATE> activeEmitters = new();

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            CheckEmitters();
        }
    }

    private void CheckEmitters()
    {
        var emitters = FindObjectsOfType<StudioEventEmitter>(true);
        foreach (var emitter in emitters)
        {
            if (emitter.EventInstance.isValid())
            {
                emitter.EventInstance.getPlaybackState(out var state);

                if (state == PLAYBACK_STATE.PLAYING)
                {
                    if (!activeEmitters.ContainsKey(emitter))
                    {
                        activeEmitters[emitter] = state;
                        Debug.Log($"▶️ [FMOD PLAYING] {GetEventPath(emitter)} on GameObject: {emitter.gameObject.name}", emitter.gameObject);
                    }
                }
                else
                {
                    if (activeEmitters.ContainsKey(emitter))
                    {
                        Debug.Log($"⏹️ [FMOD STOPPED] {GetEventPath(emitter)} on GameObject: {emitter.gameObject.name}", emitter.gameObject);
                        activeEmitters.Remove(emitter);
                    }
                }
            }
        }
    }

    private string GetEventPath(StudioEventEmitter emitter)
    {
        if (emitter.EventReference.IsNull)
            return "(null event)";

        RuntimeManager.StudioSystem.lookupPath(emitter.EventReference.Guid, out string path);
        return path;
    }
}
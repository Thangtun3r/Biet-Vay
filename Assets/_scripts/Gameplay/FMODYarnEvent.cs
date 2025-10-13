using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;
using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using STOP_MODE = FMODUnity.STOP_MODE;

public class FMODYarnEvent : MonoBehaviour
{
    private static FMODYarnEvent instance;

    private EventInstance currentEventInstance;
    private readonly List<EventInstance> activeEvents = new List<EventInstance>();

    private void Awake()
    {
        instance = this;
        // Stop everything whenever the active scene changes
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        // Safety: kill anything started by this script AND global events
        KillCurrentEvent(STOP_MODE.Immediate);
        KillTrackedEvents(STOP_MODE.Immediate);
        StopAllFMODEventsGlobal(STOP_MODE.Immediate);
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        // Fade out nicely during scene transitions
        KillCurrentEvent(STOP_MODE.AllowFadeout);
        KillTrackedEvents(STOP_MODE.AllowFadeout);
        StopAllFMODEventsGlobal(STOP_MODE.AllowFadeout);
    }

    [YarnCommand("playFMOD1shot")]
    public static void PlaySound(params string[] pathParts)
    {
        if (instance == null) { Debug.LogError("FMODYarnEvent instance not found in the scene."); return; }
        var path = string.Join(" ", pathParts);
        RuntimeManager.PlayOneShot(path);
    }

    [YarnCommand("playFMODEvent")]
    public static void PlayEvent(params string[] pathParts)
    {
        if (instance == null) { Debug.LogError("FMODYarnEvent instance not found in the scene."); return; }
        var eventPath = string.Join(" ", pathParts);

        instance.KillCurrentEvent(STOP_MODE.AllowFadeout);

        instance.currentEventInstance = RuntimeManager.CreateInstance(eventPath);
        instance.currentEventInstance.start();

        // Track active event for later cleanup
        instance.activeEvents.Add(instance.currentEventInstance);
    }

    [YarnCommand("killFMODEvent")]
    public static void KillEvent()
    {
        if (instance == null) { Debug.LogError("FMODYarnEvent instance not found in the scene."); return; }
        instance.KillCurrentEvent(STOP_MODE.AllowFadeout);
    }

    // NEW: Kill ALL events in the whole project (even ones not started here)
    // Usage in Yarn:
    //   <<killAllFMOD>>                (fade out)
    //   <<killAllFMOD immediate>>      (stop immediately)
    [YarnCommand("killAllFMOD")]
    public static void KillAllFMOD(params string[] args)
    {
        if (instance == null) { Debug.LogError("FMODYarnEvent instance not found in the scene."); return; }

        var mode = (args.Length > 0 && args[0].ToLowerInvariant().Contains("immediate"))
            ? STOP_MODE.Immediate
            : STOP_MODE.AllowFadeout;

        instance.KillCurrentEvent(mode);
        instance.KillTrackedEvents(mode);
        StopAllFMODEventsGlobal(mode);

        Debug.Log($"All FMOD events stopped ({mode}).");
    }

    private void KillCurrentEvent(STOP_MODE mode)
    {
        if (currentEventInstance.isValid())
        {
            currentEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            currentEventInstance.release();
            currentEventInstance.clearHandle();
        }
    }

    private void KillTrackedEvents(STOP_MODE mode)
    {
        for (int i = 0; i < activeEvents.Count; i++)
        {
            var e = activeEvents[i];
            if (e.isValid())
            {
                e.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                e.release();
            }
        }
        activeEvents.Clear();
    }

    // Global nuke: stops ANY playing event by using the master bus "bus:/"
    private static void StopAllFMODEventsGlobal(STOP_MODE mode)
    {
        if (RuntimeManager.StudioSystem.getBus("bus:/", out Bus masterBus) == FMOD.RESULT.OK)
        {
            masterBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        else
        {
            Debug.LogWarning("FMOD: Could not get master bus to stop all events.");
        }
    }
}

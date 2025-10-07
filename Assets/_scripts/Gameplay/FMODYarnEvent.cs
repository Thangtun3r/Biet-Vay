using UnityEngine;
using Yarn.Unity;
using FMOD.Studio;
using FMODUnity;

public class FMODYarnEvent : MonoBehaviour
{
    private static FMODYarnEvent instance;
    private EventInstance currentEventInstance;

    private void Awake() => instance = this;

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

        instance.KillCurrentEvent(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        instance.currentEventInstance = RuntimeManager.CreateInstance(eventPath);
        instance.currentEventInstance.start();
    }

    [YarnCommand("killFMODEvent")]
    public static void KillEvent()
    {
        if (instance == null) { Debug.LogError("FMODYarnEvent instance not found in the scene."); return; }
        instance.KillCurrentEvent(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    private void KillCurrentEvent(FMOD.Studio.STOP_MODE mode)
    {
        if (currentEventInstance.isValid())
        {
            currentEventInstance.stop(mode);
            currentEventInstance.release();
            currentEventInstance.clearHandle();
        }
    }
}
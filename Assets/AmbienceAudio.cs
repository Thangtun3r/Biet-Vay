using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AmbienceAudio : MonoBehaviour
{
    [Header("Primary (inspector)")]
    public EventReference ambienceEvent;                 // assign your Ambience event here
    [Header("Fallback by path (optional)")]
    public string ambienceEventPath = "event:/Ambience Outside";

    public bool attachToGameObject = true;

    private EventInstance _inst;

    private void OnEnable()
    {
        StartCoroutine(Bootstrap());
        TransitionSFX.OnTransition += OnTransitionParam;
    }

    private void OnDisable()
    {
        TransitionSFX.OnTransition -= OnTransitionParam;
        if (_inst.isValid())
        {
            _inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _inst.release();
        }
    }

    private IEnumerator Bootstrap()
    {
        // Ensure banks are loaded before creating instances
        RuntimeManager.WaitForAllLoads();
        yield return null;

        // Try create from serialized EventReference first
        if (!ambienceEvent.IsNull)
        {
            _inst = RuntimeManager.CreateInstance(ambienceEvent);
            if (_inst.isValid())
            {
                HookDebugCallbacks(_inst, "[Ambience]");
                StartInstance();
                yield break;
            }
            else
            {
                Debug.LogWarning("[Ambience] EventReference create failed; trying path fallback…");
            }
        }

        // Fallback: resolve by path (avoids stale GUIDs)
        if (!string.IsNullOrEmpty(ambienceEventPath))
        {
            var r = RuntimeManager.StudioSystem.getEvent(ambienceEventPath, out EventDescription desc);
            if (r == FMOD.RESULT.OK)
            {
                r = desc.createInstance(out _inst);
                if (r == FMOD.RESULT.OK && _inst.isValid())
                {
                    HookDebugCallbacks(_inst, "[Ambience]");
                    StartInstance();
                    yield break;
                }
                Debug.LogError($"[Ambience] createInstance from path failed: {r}");
            }
            else
            {
                Debug.LogError($"[Ambience] getEvent('{ambienceEventPath}') failed: {r} (banks/platform mismatch)");
            }
        }
        else
        {
            Debug.LogError("[Ambience] No EventReference and no path fallback provided.");
        }
    }

    private void StartInstance()
    {
        if (attachToGameObject)
        {
            var rb = GetComponent<Rigidbody>();
            RuntimeManager.AttachInstanceToGameObject(_inst, transform, rb);
        }
        var res = _inst.start();
        if (res != FMOD.RESULT.OK)
            Debug.LogWarning($"[Ambience] start() => {res}");
    }

    private void OnTransitionParam(string paramName, float value)
    {
        if (!_inst.isValid()) return;

        var res = _inst.setParameterByName(paramName, value);
        // Per manual: ERR_EVENT_NOTFOUND => no such parameter on this event
        //             ERR_INVALID_PARAM  => param not game-controlled / read-only / automatic
        if (res != FMOD.RESULT.OK)
            Debug.LogWarning($"[Ambience] setParameterByName('{paramName}', {value}) => {res}");
    }

    private void HookDebugCallbacks(EventInstance inst, string tag)
    {
        inst.setCallback((type, _event, _params) =>
        {
            switch (type)
            {
                case EVENT_CALLBACK_TYPE.STARTING:
                    Debug.Log($"{tag} CALLBACK: STARTING");
                    break;
                case EVENT_CALLBACK_TYPE.STARTED:
                    Debug.Log($"{tag} CALLBACK: STARTED");
                    break;
                case EVENT_CALLBACK_TYPE.START_FAILED:
                    Debug.LogWarning($"{tag} CALLBACK: START_FAILED (polyphony/sample data/banks?)");
                    break;
                case EVENT_CALLBACK_TYPE.STOPPED:
                    Debug.Log($"{tag} CALLBACK: STOPPED");
                    break;
            }
            return FMOD.RESULT.OK;
        }, EVENT_CALLBACK_TYPE.STARTING | EVENT_CALLBACK_TYPE.STARTED | EVENT_CALLBACK_TYPE.START_FAILED | EVENT_CALLBACK_TYPE.STOPPED);
    }
}

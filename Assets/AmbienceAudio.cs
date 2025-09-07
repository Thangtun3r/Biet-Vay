using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class AmbienceAudio : MonoBehaviour
{
    [Header("Primary (inspector)")]
    public EventReference ambienceEvent;

    [Header("Fallback by path (optional)")]
    public string ambienceEventPath = "event:/Ambience Outside";

    public bool attachToGameObject = true;

    private EventInstance _inst;
    private Coroutine _bootRoutine;

    // IMPORTANT: keep a strong reference to the callback delegate to prevent GC crashes
    private EVENT_CALLBACK _eventCallback; 

    private void OnEnable()
    {
        _bootRoutine = StartCoroutine(Bootstrap());
        TransitionSFX.OnTransition += OnTransitionParam;
    }

    private void OnDisable()
    {
        TransitionSFX.OnTransition -= OnTransitionParam;

        if (_bootRoutine != null)
        {
            StopCoroutine(_bootRoutine);
            _bootRoutine = null;
        }

        // Detach callback BEFORE releasing the instance
        if (_inst.isValid())
        {
            // Remove callback to drop native -> managed calls during shutdown/reload
            _inst.setCallback(null);
            _eventCallback = null;

            _inst.stop(STOP_MODE.ALLOWFADEOUT);
            _inst.release();
        }
    }

    private IEnumerator Bootstrap()
    {
        // If FMOD not initialized (rare during editor reload), bail out safely
#if FMOD_ENABLED
        if (!RuntimeManager.IsInitialized)
            yield break;
#endif

        RuntimeManager.WaitForAllLoads();
        yield return null; // one frame for safety

        if (!isActiveAndEnabled) yield break;

        // Try EventReference first
        if (!ambienceEvent.IsNull)
        {
            _inst = RuntimeManager.CreateInstance(ambienceEvent);
            if (_inst.isValid())
            {
                StartInstanceWithCallback();
                yield break;
            }
            else
            {
                Debug.LogWarning("[Ambience] EventReference create failed; trying path fallback…");
            }
        }

        // Fallback by path (avoids stale GUIDs)
        if (!string.IsNullOrEmpty(ambienceEventPath))
        {
            var r = RuntimeManager.StudioSystem.getEvent(ambienceEventPath, out EventDescription desc);
            if (r == FMOD.RESULT.OK && desc.isValid())
            {
                r = desc.createInstance(out _inst);
                if (r == FMOD.RESULT.OK && _inst.isValid())
                {
                    StartInstanceWithCallback();
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

    private void StartInstanceWithCallback()
    {
        if (attachToGameObject)
        {
            var rb = GetComponent<Rigidbody>();
            RuntimeManager.AttachInstanceToGameObject(_inst, transform, rb);
        }

        // --- SAFE CALLBACK SETUP ---
        // keep a strong reference in a field so GC won't collect it on domain reload
        _eventCallback = (type, _evt, _params) =>
        {
            // optional: keep minimal logging; remove if you want it ultra-quiet
            // if (type == EVENT_CALLBACK_TYPE.START_FAILED)
            //     Debug.LogWarning("[Ambience] START_FAILED");
            return FMOD.RESULT.OK;
        };
        _inst.setCallback(_eventCallback,
            EVENT_CALLBACK_TYPE.STARTING |
            EVENT_CALLBACK_TYPE.STARTED |
            EVENT_CALLBACK_TYPE.START_FAILED |
            EVENT_CALLBACK_TYPE.STOPPED);

        var res = _inst.start();
        if (res != FMOD.RESULT.OK)
            Debug.LogWarning($"[Ambience] start() => {res}");
    }

    private void OnTransitionParam(string paramName, float value)
    {
        if (_inst.isValid())
        {
            var r = _inst.setParameterByName(paramName, value);
            // You can comment this out for max silence:
            // if (r != FMOD.RESULT.OK) Debug.LogWarning($"Param '{paramName}' => {r}");
        }
    }
}

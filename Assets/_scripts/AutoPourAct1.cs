using System.Collections;
using UnityEngine;

public class AutoPourAct1 : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The WordPoolTrigger that does the actual pour/clear logic")]
    public WordPoolTrigger poolTrigger;

    [Tooltip("Assign your WordsPackage asset named “Act1PsudoTrigger” here")]
    public WordsPackage act1PsudoTrigger;

    private bool _hasPoured = false;

    private void Start()
    {
        // Kick off the one-frame delay coroutine
        StartCoroutine(PourOnceWhenReady());
    }

    private IEnumerator PourOnceWhenReady()
    {
        // Give everything else one frame to finish Awake()/Start()
        yield return null;

        if (_hasPoured)
            yield break;

        if (poolTrigger == null)
        {
            Debug.LogError("AutoPourAct1: poolTrigger reference is missing.");
            yield break;
        }

        // This will ClearPool (if configured) → pop-out tween off → pour Act1PsudoTrigger
        poolTrigger.LoadAndPour(act1PsudoTrigger);
        _hasPoured = true;
    }
}
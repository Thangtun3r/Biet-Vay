using UnityEngine;
using System.Collections;
using Gameplay;

public class VisualHandler : MonoBehaviour
{
    [Header("Visual Pooling")]
    public WordsPooling visualPooling; // Assign a separate pool of your VisualWord prefab
    public float delayBeforeSpawning = 0.1f; // You can tweak this in Inspector

    private void OnEnable()
    {
        WordPoolManager.OnWordCreated += HandleWordCreated;
    }

    private void OnDisable()
    {
        WordPoolManager.OnWordCreated -= HandleWordCreated;
    }

    private void HandleWordCreated(GameObject logicWord)
    {
        StartCoroutine(SpawnVisualsWithDelay(logicWord));
    }

    private IEnumerator SpawnVisualsWithDelay(GameObject logicWord)
    {
        yield return new WaitForSeconds(delayBeforeSpawning);

        var logicRoot = logicWord.transform;
        var words = logicWord.GetComponent<Words>();

        foreach (Transform child in logicRoot)
        {
            GameObject visObj = visualPooling.GetPooledObject();
            visObj.transform.SetParent(transform, false);

            var vw = visObj.GetComponent<VisualWord>();
            var wvi = visObj.GetComponent<WordVisualInteraction>();

            vw.logicWordObject = logicWord;
            vw.target = child;

            //Wtf we can fucking subscribe to the event from here in the factory ???
            //Holyshit
            words.BeganDrag += wvi.HandleBeginDragVisual;
            words.Dragged   += wvi.HandleDragVisual;
            words.PointerEntered += wvi.HandleHoverVisual;
            words.PointerExited  += wvi.HandleExitVisual;
            words.PointerUpped += wvi.HandleExitVisual;
            words.EndedDrag += wvi.HandleExitVisual;
        }
    }

    
}
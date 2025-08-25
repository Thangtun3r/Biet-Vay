using UnityEngine;
using System.Collections;

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
        yield return new WaitForSeconds(delayBeforeSpawning); // Wait a short time
        
        var logicRoot = logicWord.transform;

        foreach (Transform child in logicRoot)
        {
            GameObject visObj = visualPooling.GetPooledObject();
            visObj.transform.SetParent(transform, false);

            var vw = visObj.GetComponent<VisualWord>();
            vw.logicWordObject = logicWord;
            vw.target = child;
        }
    }
    
}
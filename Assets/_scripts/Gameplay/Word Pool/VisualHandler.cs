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

    private void HandleWordCreated(int id, string word, Transform logicRoot)
    {
        StartCoroutine(SpawnVisualsWithDelay(id, word, logicRoot));
    }

    private IEnumerator SpawnVisualsWithDelay(int id, string word, Transform logicRoot)
    {
        yield return new WaitForSeconds(delayBeforeSpawning); // Wait a short time

        foreach (Transform child in logicRoot)
        {
            GameObject visObj = visualPooling.GetPooledObject();
            visObj.transform.SetParent(transform, false);

            var visualWordID = visObj.GetComponent<WordID>();
            visualWordID.id = id;
            visualWordID.word = word;
            visualWordID.wordText.text = word;

            var vw = visObj.GetComponent<VisualWord>();
            vw.target = child;
        }
    }
}
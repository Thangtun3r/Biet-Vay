using UnityEngine;

public class VisualHandler : MonoBehaviour
{
    [Header("Visual Pooling")]
    public WordsPooling visualPooling;      // assign a separate pool of your VisualWord prefab

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
        // Loop over every direct child (or, if you want grandchildren too, use GetComponentsInChildren)
        foreach (Transform child in logicRoot)
        {
            // 1. Pull from the visual pool
            GameObject visObj = visualPooling.GetPooledObject();
            visObj.transform.SetParent(transform, false);

            // 2. Copy your WordID data into the visual prefab
            var visualWordID = visObj.GetComponent<WordID>();
            visualWordID.id   = id;
            visualWordID.word = word;
            visualWordID.wordText.text = word;

            // 3. Tell the VisualWord to follow *this* child
            var vw = visObj.GetComponent<VisualWord>();
            vw.target = child;
            // (Optionally tweak its offsets per‐child if you need)
        }
    }


}
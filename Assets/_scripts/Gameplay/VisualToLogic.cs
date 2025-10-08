using UnityEngine;
using System;

public class VisualToLogic : MonoBehaviour
{
    public GameObject visualWordObject;

    // Declare a non-static event to broadcast the visualWordObject
    public event Action<GameObject> OnVisualWordSet;

    // Method to set the visualWordObject and trigger the event
    public void SetVisualWord(GameObject visualWord)
    {
        visualWordObject = visualWord;

        // Trigger the event to notify other scripts
        OnVisualWordSet?.Invoke(visualWordObject);
    }
}
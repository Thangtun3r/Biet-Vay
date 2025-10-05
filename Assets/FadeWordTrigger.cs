using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class IntEvent : UnityEvent<int> {}

public class FadeWordTrigger : MonoBehaviour
{
    [Header("Optional: expose a UnityEvent for Inspector hookups")]
    public IntEvent onFadeWord;

    // This is what the commander subscribes to
    public void HandleFadeWord(int id)
    {
        // put your response here (fade, swap, etc.)
        onFadeWord?.Invoke(id);
        // e.g. if you want to drive a SpriteColorChanger too:
        // var scc = GetComponent<SpriteColorChanger>();
        // if (scc != null) scc.SetType(id);
    }
}
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class YarnNodeTriggerIsTrigger : MonoBehaviour
{
    [SerializeField] private string nodeName;
    private bool playerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!(other.attachedRigidbody != null 
                ? other.attachedRigidbody.CompareTag("Player")
                : other.transform.root.CompareTag("Player")))
            return;

        if (playerInside) return;      // already handled this presence
        playerInside = true;

        YarnDialogueEventBridge.CallYarnEvent(nodeName);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!(other.attachedRigidbody != null 
                ? other.attachedRigidbody.CompareTag("Player")
                : other.transform.root.CompareTag("Player")))
            return;

        playerInside = false;          // allow it to trigger again on next entry
    }
}
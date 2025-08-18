using UnityEngine;
using TMPro; // Make sure you have TextMeshPro imported

public class YarnDebugConsole : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField inputField;

    private void Update()
    {
        // If user presses Enter/Return while inputField is focused
        if (inputField != null && inputField.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            string nodeName = inputField.text.Trim();
            if (!string.IsNullOrEmpty(nodeName))
            {
                JumpToNode(nodeName);
                inputField.text = ""; // clear after use
                inputField.DeactivateInputField(); // remove focus
            }
        }
    }

    private void JumpToNode(string nodeName)
    {
        Debug.Log($"[YarnDebugConsole] Jumping to Yarn node: {nodeName}");
        YarnDialogueEventBridge.CallYarnEvent(nodeName);
    }
}
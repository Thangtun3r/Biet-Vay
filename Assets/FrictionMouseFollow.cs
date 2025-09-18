using UnityEngine;

public class FrictionMouseFollow : MonoBehaviour
{
    public bool canFollowMouse = false; // Control to start/stop following the mouse
    public float followSpeed = 5f; // Speed at which the object will follow the mouse

    private RectTransform rectTransform;
    private WordVisualInteraction wordVisualInteraction;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        wordVisualInteraction = GetComponent<WordVisualInteraction>(); // Reference to WordVisualInteraction
    }

    void Update()
    {
        if (canFollowMouse)
        {
            FollowMouse();
        }
    }

    private void FollowMouse()
    {
        // Stop any ongoing tweens and reset the position to base Y
        if (wordVisualInteraction != null)
        {
            wordVisualInteraction.KillCurrentTween(); // Stop current tweens
            wordVisualInteraction.ResetToBaseY(); // Reset position
        }

        // Get the mouse position in screen space
        Vector2 mousePosition = Input.mousePosition;

        // Smoothly move the object towards the mouse position
        rectTransform.position = Vector2.Lerp(rectTransform.position, mousePosition, followSpeed * Time.deltaTime);
    }

    public void StopFollowMouse()
    {
        canFollowMouse = false;
    }
}
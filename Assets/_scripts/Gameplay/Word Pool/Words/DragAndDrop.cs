using UnityEngine;
using UnityEngine.UI;

public class DragAndDrop : MonoBehaviour
{
    public float spacing = 12f;
    [SerializeField] private HorizontalLayoutGroup layoutElement;

    private int originalPreferredWidth;

    private void Start()
    {
        if (layoutElement != null)
            originalPreferredWidth = layoutElement.padding.left;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Detector"))
        {
            SpacingOut();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Detector"))
        {
            ResetToOriginalSize();
        }
    }

    private void SpacingOut()
    {
        if (layoutElement != null)
        {
            layoutElement.padding = new RectOffset((int)spacing, 0, 0, 0);
            ForceRefresh();
        }
    }
    

    public void ResetToOriginalSize()
    {
        if (layoutElement != null)
        {
            layoutElement.padding = new RectOffset(originalPreferredWidth, 0, 0, 0);
            ForceRefresh();
        }
    }

    private void ForceRefresh()
    {
        // Force Unity to recalculate the layout immediately
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutElement.GetComponent<RectTransform>());
    }
}
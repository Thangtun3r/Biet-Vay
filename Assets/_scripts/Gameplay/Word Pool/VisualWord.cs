using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VisualWord : MonoBehaviour
{
    [Header("Logic References")]
    public GameObject logicWordObject;   // The real "logic" word object in the layout
    public Transform target;             // A child of the logic word we want to follow
    public string logicWordText;         // Just a copy of the logic word text (for debugging)
    private bool isSpacing = false;

    [Header("Visual References")]
    public TextMeshProUGUI text;         // The TMP text on the visual
    public float followSpeed = 10f;      // How quickly this visual moves to match its target
    public float horizontalOffset = 0f;  // Optional side offset (positive = right, negative = left)

    
    public HorizontalLayoutGroup layoutElement;
    public int spacing = 5;
    private int originalPreferredWidth;
    
    private DragAndDrop dragAndDrop;
    



    private void Start()
    {
        if (layoutElement != null)
            originalPreferredWidth = layoutElement.padding.left;
        
    }
    
    private void Update()
    {
        // Keep the visual's text synced with the logic word's text
        if (text != null && logicWordObject != null)
        {
            var wordID = logicWordObject.GetComponent<WordID>();
            if (wordID != null)
                text.text = wordID.word;
            var dragAndDrop = logicWordObject.GetComponentInChildren<DragAndDrop>();
            isSpacing = dragAndDrop.isSpacing;

        }
        SpacingOut();
        ResetToOrigianlSize();
    }

    private void SpacingOut()
    {
        if (layoutElement != null && isSpacing)
        {
            layoutElement.padding = new RectOffset((int)spacing, 0, 0, 0);
            ForceRefresh();
        }
    }

    private void ResetToOrigianlSize()
    {
        if (layoutElement != null && isSpacing == false)
        {
            layoutElement.padding = new RectOffset(originalPreferredWidth, 0, 0, 0);
            ForceRefresh();
        }
    }

    private void LateUpdate()
    {
        if (!target) return;

        // Update the visual's position and size so it matches the logic target
        FollowTargetRect();
    }

    /// <summary>
    /// This method keeps the visual overlay aligned with the logic word.
    /// 
    /// Steps in plain English:
    /// 1. Ask Unity for the 4 corners of the target rect in world space.
    /// 2. Convert those corners into this visual's parent's local space.
    ///    (so we’re in the same coordinate system).
    /// 3. Work out the center and size of the target.
    /// 4. Smoothly move the visual to that center and resize it to match.
    /// </summary>
    private void FollowTargetRect()
    {
        var rt = (RectTransform)transform;       // The visual rect (this object)
        var parentRt = (RectTransform)rt.parent; // The container that holds all visuals
        var targetRt = (RectTransform)target;    // The logic rect we want to copy

        // --- Step 1: get the target’s world corners
        var worldCorners = new Vector3[4];
        targetRt.GetWorldCorners(worldCorners);

        // --- Step 2: convert world corners into parent local space
        var canvas = parentRt.GetComponentInParent<Canvas>();
        var cam = (canvas && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            ? canvas.worldCamera
            : null;

        Vector2 Local(Vector3 w)
        {
            Vector2 sp = RectTransformUtility.WorldToScreenPoint(cam, w);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, sp, cam, out var lp);
            return lp;
        }

        Vector2 bottomLeft = Local(worldCorners[0]);
        Vector2 topRight   = Local(worldCorners[2]);

        // --- Step 3: compute center + size
        Vector2 center = (bottomLeft + topRight) * 0.5f;
        Vector2 size   = (topRight - bottomLeft); // width = x, height = y

        // --- Step 4: apply center + size to the visual
        rt.anchoredPosition = Vector2.Lerp(
            rt.anchoredPosition,
            center + new Vector2(horizontalOffset, 0f),
            followSpeed * Time.deltaTime
        );

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   size.y);
    }
    private void ForceRefresh()
    {
        // Force Unity to recalculate the layout immediately
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutElement.GetComponent<RectTransform>());
    }
}

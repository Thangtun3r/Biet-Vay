using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Layout/Flow Layout Group")]
public class FlowLayoutGroup : LayoutGroup
{
    public float spacingX = 5f;
    public float spacingY = 5f;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        CalculateLayout();
    }

    public override void CalculateLayoutInputVertical()
    {
        CalculateLayout();
    }

    public override void SetLayoutHorizontal()
    {
        PlaceChildren();
    }

    public override void SetLayoutVertical()
    {
        PlaceChildren();
    }

    private void CalculateLayout()
    {
        float width = rectTransform.rect.width;
        float x = padding.left;
        float y = padding.top;
        float rowHeight = 0;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform child = rectChildren[i];
            float w = LayoutUtility.GetPreferredSize(child, 0);
            float h = LayoutUtility.GetPreferredSize(child, 1);

            // wrap to next line?
            if (x + w + padding.right > width)
            {
                x = padding.left;
                y += rowHeight + spacingY;
                rowHeight = 0;
            }

            // advance
            x += w + spacingX;
            rowHeight = Mathf.Max(rowHeight, h);
        }

        // total height needed
        y += rowHeight + padding.bottom;
        SetLayoutInputForAxis(width, width, -1, 0);
        SetLayoutInputForAxis(y, y, -1, 1);
    }

    private void PlaceChildren()
    {
        float width = rectTransform.rect.width;
        float x = padding.left;
        float y = padding.top;
        float rowHeight = 0;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform child = rectChildren[i];
            float w = LayoutUtility.GetPreferredSize(child, 0);
            float h = LayoutUtility.GetPreferredSize(child, 1);

            if (x + w + padding.right > width)
            {
                x = padding.left;
                y += rowHeight + spacingY;
                rowHeight = 0;
            }

         
            SetChildAlongAxis(child, 0, x, w);
            SetChildAlongAxis(child, 1, y, h);

            x += w + spacingX;
            rowHeight = Mathf.Max(rowHeight, h);
        }
    }
}

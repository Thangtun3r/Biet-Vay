using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class UIColliderGenerator : MonoBehaviour
{
    public enum ColliderType { Box, Polygon }
    [Tooltip("Type of 2D collider to generate")]
    public ColliderType type = ColliderType.Box;

    [Tooltip("Auto-regenerate when RectTransform or Sprite changes")]
    public bool autoGenerate = true;

    RectTransform rt;
    Image img;
    BoxCollider2D box;
    PolygonCollider2D poly;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        img = GetComponent<Image>();
        // only auto-gen if no BoxCollider2D already exists
        if (autoGenerate && GetComponent<BoxCollider2D>() == null)
            Generate();
    }

    void OnValidate()
    {
        // skip if user has already added their own BoxCollider2D
        if (autoGenerate && rt != null && GetComponent<BoxCollider2D>() == null)
            Generate();
    }

    void OnRectTransformDimensionsChange()
    {
        if (autoGenerate && rt != null && GetComponent<BoxCollider2D>() == null)
            Generate();
    }

    [ContextMenu("Generate Collider")]
    public void Generate()
    {
        // remove whichever collider we don't need
        if (type == ColliderType.Box)
        {
            if (poly) DestroyImmediate(poly);
            if (!box) box = gameObject.AddComponent<BoxCollider2D>();
            var size = rt.rect.size;
            box.size = size;
            box.offset = rt.rect.center;
        }
        else // Polygon
        {
            if (box) DestroyImmediate(box);
            if (!poly) poly = gameObject.AddComponent<PolygonCollider2D>();

            if (img != null && img.sprite != null)
            {
                Sprite s = img.sprite;
                int shapeCount = s.GetPhysicsShapeCount();
                poly.pathCount = shapeCount;
                var points = new List<Vector2>();
                for (int i = 0; i < shapeCount; i++)
                {
                    points.Clear();
                    s.GetPhysicsShape(i, points);
                    var verts = points.Select(p => p / s.pixelsPerUnit).ToArray();
                    poly.SetPath(i, verts);
                }
                poly.transform.localScale = Vector3.one * (rt.rect.size.x / s.bounds.size.x);
            }
            else
            {
                Debug.LogWarning(name + ": no sprite physics shape found; switching to box collider.");
                type = ColliderType.Box;
                Generate();
            }
        }
    }
}

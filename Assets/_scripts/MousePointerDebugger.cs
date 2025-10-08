// UIRaycastOcclusionDebugger.cs
// Drop on any active object in your scene (needs an EventSystem present).
// Press F2 to toggle overlay.

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DefaultExecutionOrder(10000)]
public class UIRaycastOcclusionDebugger : MonoBehaviour
{
    [Header("Overlay")]
    public bool show = true;
    public KeyCode toggleKey = KeyCode.F2;
    public int fontSize = 13;
    public int maxResults = 30;
    public bool highlightTopmost = true;

    [Header("Ray settings")]
    public Camera uiCamera;                 // If null, will use EventSystem module camera or Camera.main
    public float worldRayDistance = 1000f;
    public LayerMask worldMask = ~0;

    private readonly List<RaycastResult> _uiResults = new();
    private readonly PointerEventData _ped = new(EventSystem.current);
    private GUIStyle _style;
    private GameObject _lastTopGO;

    void Awake()
    {
        if (_style == null)
            _style = new GUIStyle { fontSize = fontSize, normal = { textColor = Color.white } };

        if (uiCamera == null) uiCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) show = !show;
        if (_style.fontSize != fontSize) _style.fontSize = fontSize;

        if (EventSystem.current != null && _ped != null)
        {
            _ped.Reset();
            _ped.position = Input.mousePosition;
            _uiResults.Clear();
            EventSystem.current.RaycastAll(_ped, _uiResults);
        }
    }

    void OnGUI()
    {
        if (!show) return;

        var sb = new StringBuilder(1024);

        // Header
        sb.AppendLine("<b>UI Raycast Occlusion Debugger</b>");
        sb.AppendLine($"EventSystem: {(EventSystem.current ? EventSystem.current.name : "<none>")}  |  Selected: {EventSystem.current?.currentSelectedGameObject?.name ?? "<null>"}");
        sb.AppendLine($"Mouse: {Input.mousePosition.x:0},{Input.mousePosition.y:0}  |  Cursor visible={Cursor.visible} lock={Cursor.lockState}");

        // Which GraphicRaycaster(s) are present
        var raycasters = FindObjectsOfType<GraphicRaycaster>(true);
        sb.AppendLine($"GraphicRaycasters ({raycasters.Length}):");
        foreach (var gr in raycasters)
        {
            var canvas = gr.GetComponent<Canvas>();
            sb.AppendLine($"  - {gr.name}  canvas={CanvasMode(canvas)} sorting={canvas.sortingLayerName}/{canvas.sortingOrder}  blocking={gr.blockingObjects}  mask={gr.blockingMask.value}");
        }

        // UI raycast results
        sb.AppendLine($"\n<b>UI Hits (top → bottom, count={Mathf.Min(_uiResults.Count, maxResults)})</b>");
        GameObject topmost = null;
        for (int i = 0; i < _uiResults.Count && i < maxResults; i++)
        {
            var r = _uiResults[i];
            var go = r.gameObject;
            if (i == 0) topmost = go;

            var graphic = go.GetComponent<Graphic>();
            var rt = go.GetComponent<RectTransform>();
            var path = GetPath(go.transform);

            // graphic data
            string ginfo = graphic ? $"raycastTarget={graphic.raycastTarget} alpha={(graphic is MaskableGraphic mg ? mg.color.a : 1f):0.###} enabled={graphic.enabled}" : "no Graphic";
            // canvas group accumulate
            bool blocksRay = true;
            float combinedAlpha = 1f;
            bool ignoreParentGroups = false;
            foreach (var cg in go.GetComponentsInParent<CanvasGroup>(true))
            {
                if (cg.ignoreParentGroups) { ignoreParentGroups = true; break; }
                combinedAlpha *= cg.alpha;
                if (!cg.blocksRaycasts) blocksRay = false;
            }

            sb.AppendLine($"{i,2}. {go.name}  [{path}]  dist={r.distance:0.##}  sortOrder={r.sortingOrder}  module={r.module?.GetType().Name}");
            sb.AppendLine($"    Graphic: {ginfo}");
            sb.AppendLine($"    CanvasGroup: blocksRaycasts={blocksRay} combinedAlpha={combinedAlpha:0.###} ignoreParentGroups={ignoreParentGroups}");
            sb.AppendLine($"    ActiveInHierarchy={go.activeInHierarchy} layer={LayerMask.LayerToName(go.layer)} rect={RectString(rt)}");
        }

        // World (Physics) blockers if your raycaster blocks world objects
        var cam = ResolveUICamera();
        if (cam != null)
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit3D, worldRayDistance, worldMask, QueryTriggerInteraction.Collide))
            {
                sb.AppendLine($"\n<b>Physics3D Hit</b>: {hit3D.collider.name}  layer={LayerMask.LayerToName(hit3D.collider.gameObject.layer)}  dist={hit3D.distance:0.##}");
            }
#if UNITY_2D
            var hit2D = Physics2D.GetRayIntersection(ray, worldRayDistance, worldMask);
            if (hit2D.collider)
            {
                sb.AppendLine($"<b>Physics2D Hit</b>: {hit2D.collider.name}  layer={LayerMask.LayerToName(hit2D.collider.gameObject.layer)}");
            }
#endif
        }

        // Draw panel
        const float w = 900f, h = 420f;
        GUI.Box(new Rect(10, 10, w, h), GUIContent.none);
        var area = new Rect(20, 20, w - 20, h - 20);
        GUILayout.BeginArea(area);
        GUILayout.Label(sb.ToString(), _style);
        GUILayout.EndArea();

        // highlight topmost (likely occluder)
        if (highlightTopmost && topmost)
        {
            _lastTopGO = topmost;
            DrawRectTransformOutline(topmost.GetComponent<RectTransform>(), Color.magenta);
        }
    }

    private Camera ResolveUICamera()
    {
        if (uiCamera) return uiCamera;
        if (EventSystem.current && EventSystem.current.currentInputModule is BaseInputModule bim)
        {
            // Most modules expose a "inputOverride" or use Camera.main; fallback:
            return Camera.main;
        }
        return Camera.main;
    }

    // Helpers

    private static string CanvasMode(Canvas c)
    {
        if (!c) return "<no canvas>";
        return c.renderMode switch
        {
            RenderMode.ScreenSpaceOverlay => "Overlay",
            RenderMode.ScreenSpaceCamera => "ScreenSpaceCamera",
            RenderMode.WorldSpace => "WorldSpace",
            _ => c.renderMode.ToString()
        };
    }

    private static string GetPath(Transform t)
    {
        var stack = new List<string>(8);
        while (t != null) { stack.Add(t.name); t = t.parent; }
        stack.Reverse();
        return string.Join("/", stack);
    }

    private static string RectString(RectTransform rt)
    {
        if (!rt) return "<no rect>";
        return $"pos={rt.position} size={rt.rect.size}";
    }

    private static void DrawRectTransformOutline(RectTransform rt, Color color)
    {
        if (!rt) return;
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        // convert world to GUI
        for (int i = 0; i < 4; i++)
        {
            var sp = RectTransformUtility.WorldToScreenPoint(Camera.main, corners[i]);
            corners[i] = new Vector3(sp.x, Screen.height - sp.y, 0);
        }

        var tex = Texture2D.whiteTexture;
        GUI.color = color;
        DrawLine(corners[0], corners[1], 2, tex);
        DrawLine(corners[1], corners[2], 2, tex);
        DrawLine(corners[2], corners[3], 2, tex);
        DrawLine(corners[3], corners[0], 2, tex);
        GUI.color = Color.white;
    }

    private static void DrawLine(Vector2 a, Vector2 b, float width, Texture2D tex)
    {
        var diff = b - a;
        float len = diff.magnitude;
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        var rect = new Rect(a.x, a.y - width * 0.5f, len, width);
        var m = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, a);
        GUI.DrawTexture(rect, tex);
        GUI.matrix = m;
    }
}

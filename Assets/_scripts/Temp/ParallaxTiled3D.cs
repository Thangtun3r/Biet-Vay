using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;    // for PrefabUtility + Handles.Label
#endif

[ExecuteAlways]
public class ConveyorLineLooper : MonoBehaviour
{
    [Header("Path (Straight)")]
    public Transform pointA;
    public Transform pointB;

    [Header("Tile Prefabs (Random)")]
    [Tooltip("Pick one at random for each tile. If empty, nothing will spawn.")]
    public Transform[] tilePrefabs;
    [Tooltip("Use a fixed seed for deterministic random choices.")]
    public bool useDeterministicSeed = true;
    public int randomSeed = 12345;

    [Header("Motion")]
    public float speed = 2f;   // units/sec A -> B

    [Header("Tiling")]
    [Tooltip("If 0, inferred per selected prefab (its local +Z). Each tile is scaled so its Z length = tileLenScaled.")]
    public float prefabTileLength = 0f;
    [Range(0, 0.5f)] public float spacingRatio = 0f;
    public bool scaleTilesToFill = true;
    public int extraTiles = 2;

    [Header("Lateral Offset (toggleable)")]
    public bool enableLateralOffset = true;
    [Tooltip("Minimum lateral offset (left is negative, right is positive).")]
    public float lateralOffsetMin = -0.25f;
    [Tooltip("Maximum lateral offset (left is negative, right is positive).")]
    public float lateralOffsetMax = 0.25f;

    // ---- Gizmos ----
    [Header("Gizmos")]
    public bool showGizmos = true;
    public string beltName = "Belt";
    public Color gizmoColor = new Color(0.15f, 0.9f, 0.9f, 1f);
    public Color labelColor = Color.white;
    [Range(0.5f, 2.5f)] public float handleSize = 1.0f;

    struct Tile
    {
        public Transform tr;
        public float phase;          // distance from A along the path [0..segmentLen)
        public float lateralOffset;  // constant left/right offset for this tile
        public float baseZLen;       // original chosen prefab's base length along +Z
    }

    List<Tile> _tiles = new();
    float _segmentLen, _tileLenScaled, _stride;
    Vector3 _a, _b, _dir;
    System.Random _rng;

    void Start()
    {
        if (Application.isPlaying) Build();
    }

    void OnValidate()
    {
        if (speed < 0f) speed = 0f;
        if (extraTiles < 0) extraTiles = 0;
        if (lateralOffsetMax < lateralOffsetMin)
            lateralOffsetMax = lateralOffsetMin;
    }

    // ---------- Build / Rebuild ----------
    void Build()
    {
        if (!pointA || !pointB) return;
        if (tilePrefabs == null || tilePrefefsEmpty()) return;

        // Clear old
        foreach (var t in _tiles) if (t.tr) DestroyImmediate(t.tr.gameObject);
        _tiles.Clear();

        // Seed RNG
        _rng = useDeterministicSeed ? new System.Random(randomSeed) : new System.Random();

        _a = pointA.position;
        _b = pointB.position;
        _dir = (_b - _a).normalized;
        _segmentLen = Mathf.Max(Vector3.Distance(_a, _b), 0.0001f);

        // Decide stride (tile + gap). We solve using an initial guess N and scale length.
        // We use a representative base length: average of prefab lengths (or provided override).
        float representativeBase = Mathf.Max(0.0001f, (prefabTileLength > 0f ? prefabTileLength : AvgPrefabZLength()));
        _tileLenScaled = representativeBase;
        float gap = spacingRatio * _tileLenScaled;

        if (scaleTilesToFill)
        {
            int N = Mathf.Max(1, Mathf.RoundToInt(_segmentLen / (representativeBase * (1f + spacingRatio))));
            float denom = N + (N - 1) * spacingRatio;
            _tileLenScaled = (_segmentLen / Mathf.Max(denom, 1e-4f));
            gap = spacingRatio * _tileLenScaled;
        }

        _stride = _tileLenScaled + gap;

        int needed = Mathf.CeilToInt((_segmentLen + _stride) / _stride) + extraTiles;

        for (int i = 0; i < needed; i++)
        {
            // Pick a prefab
            Transform src = tilePrefabs[_rng.Next(tilePrefabs.Length)];
            if (!src) continue;

            // Instantiate (works in Edit & Play mode)
            Transform tr = Application.isPlaying
                ? Instantiate(src, transform)
                : (Transform)PrefabUtility.InstantiatePrefab(src, transform);

            // Determine this prefab's base Z length
            float baseLen = Mathf.Max(0.0001f, (prefabTileLength > 0f ? prefabTileLength : GetPrefabZLength(src)));

            // Scale along +Z so this tile matches the global tileLenScaled
            Vector3 ls = tr.localScale;
            ls.z *= (_tileLenScaled / baseLen);
            tr.localScale = ls;

            // Phase & lateral offset
            float phase = (i * _stride) % _segmentLen;
            float lateral = 0f;
            if (enableLateralOffset)
                lateral = RandomRange(_rng, lateralOffsetMin, lateralOffsetMax);

            var tile = new Tile { tr = tr, phase = phase, lateralOffset = lateral, baseZLen = baseLen };
            Place(tile.tr, tile.phase, tile.lateralOffset);
            _tiles.Add(tile);
        }
    }

    bool tilePrefefsEmpty()
    {
        if (tilePrefabs == null || tilePrefabs.Length == 0) return true;
        for (int i = 0; i < tilePrefabs.Length; i++)
            if (tilePrefabs[i] != null) return false;
        return true;
    }

    // ---------- Update ----------
    void Update()
    {
        if (!Application.isPlaying) return;
        if (_tiles.Count == 0 || !pointA || !pointB) return;

        _a = pointA.position;
        _b = pointB.position;
        _dir = (_b - _a).normalized;
        _segmentLen = Mathf.Max(Vector3.Distance(_a, _b), 0.0001f);

        float delta = speed * Time.deltaTime;

        for (int i = 0; i < _tiles.Count; i++)
        {
            var t = _tiles[i];
            t.phase += delta;
            if (t.phase >= _segmentLen) t.phase -= _segmentLen;
            Place(t.tr, t.phase, t.lateralOffset);
            _tiles[i] = t;
        }
    }

    // ---------- Placement ----------
    void Place(Transform tr, float distance, float lateral)
    {
        Vector3 pos = _a + _dir * distance;

        // Right vector (perpendicular, horizontal). If your belts tilt, use transform.up instead of Vector3.up.
        Vector3 right = Vector3.Cross(Vector3.up, _dir).normalized;
        pos += right * lateral;

        tr.position = pos;
        tr.rotation = Quaternion.LookRotation(_dir, Vector3.up);
    }

    // ---------- Length helpers ----------
    float AvgPrefabZLength()
    {
        float sum = 0f; int n = 0;
        foreach (var p in tilePrefabs)
        {
            if (!p) continue;
            sum += GetPrefabZLength(p);
            n++;
        }
        return (n > 0) ? sum / n : 1f;
    }

    float GetPrefabZLength(Transform prefab)
    {
        var mf = prefab.GetComponentInChildren<MeshFilter>();
        if (mf && mf.sharedMesh)
            return Mathf.Abs(mf.sharedMesh.bounds.size.z * mf.transform.localScale.z);

        var r = prefab.GetComponentInChildren<Renderer>();
        if (r) return Mathf.Abs(Vector3.Dot(r.bounds.size, prefab.forward.normalized));

        return 1f;
    }

    static float RandomRange(System.Random rng, float min, float max)
    {
        if (min == max) return min;
        double t = rng.NextDouble();
        return (float)(min + (max - min) * t);
    }

    // ---------- Always-on Gizmos ----------
    void OnDrawGizmos()
    {
        if (!showGizmos || !pointA || !pointB) return;

        Vector3 a = pointA.position;
        Vector3 b = pointB.position;
        Vector3 dir = (b - a).normalized;
        float len = Vector3.Distance(a, b);

        Gizmos.color = gizmoColor;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawSphere(a, 0.04f * handleSize);
        Gizmos.DrawSphere(b, 0.04f * handleSize);

        // Direction arrow
        Vector3 mid = Vector3.Lerp(a, b, 0.5f);
        Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
        float arrow = Mathf.Clamp(len * 0.06f * handleSize, 0.05f, 0.6f);
        Gizmos.DrawLine(mid, mid + dir * arrow);
        Gizmos.DrawLine(mid + dir * arrow, mid + dir * arrow - dir * arrow * 0.6f + right * arrow * 0.4f);
        Gizmos.DrawLine(mid + dir * arrow, mid + dir * arrow - dir * arrow * 0.6f - right * arrow * 0.4f);

        #if UNITY_EDITOR
        var prev = Handles.color;
        Handles.color = labelColor;
        GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
        style.normal.textColor = labelColor;
        Handles.Label(mid + Vector3.up * (0.1f * handleSize), beltName, style);
        Handles.color = prev;
        #endif
    }

    // ---------- Editor helpers ----------
    [ContextMenu("Rebuild Now")]
    void RebuildNow()
    {
        Build();
    }
}

using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class KiteString : MonoBehaviour
{
    [Header("Endpoints")]
    public Transform pointA;                  // Hand / Reel
    public Transform pointB;                  // Kite
    public Vector3 localOffsetA;              // Attach offset on A
    public Vector3 localOffsetB;              // Attach offset on B

    [Header("Look")]
    public float width = 0.01f;
    public Gradient color = null;

    [Header("Curve (sag)")]
    public bool useSag = true;
    [Range(2, 128)] public int segments = 24;
    [Tooltip("How much the string sags downward in world space.")]
    public float sagAmount = 0.5f;
    [Tooltip("Extra sag as the distance grows (added per meter).")]
    public float sagPerMeter = 0.1f;
    [Tooltip("World-space direction of gravity/sag.")]
    public Vector3 sagDirection = Vector3.down;

    [Header("Physics sampling")]
    public bool sampleFromRigidbodies = true; // Helps with jitter
    public Rigidbody rbA;
    public Rigidbody rbB;

    LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        if (color != null) lr.colorGradient = color;
        lr.widthCurve = AnimationCurve.Constant(0, 1, width);
        lr.widthMultiplier = 1f;
    }

    void Update()
    {
        if (!pointA || !pointB) return;

        // Get world attach points (supports per-object offsets)
        Vector3 a = pointA.TransformPoint(localOffsetA);
        Vector3 b = pointB.TransformPoint(localOffsetB);

        // If sampling rigidbodies, prefer rb positions to reduce 1-frame delay with physics
        if (Application.isPlaying && sampleFromRigidbodies)
        {
            if (!rbA) rbA = pointA.GetComponent<Rigidbody>();
            if (!rbB) rbB = pointB.GetComponent<Rigidbody>();
            if (rbA) a = rbA.position + pointA.TransformVector(localOffsetA);
            if (rbB) b = rbB.position + pointB.TransformVector(localOffsetB);
        }

        // Straight line
        if (!useSag || segments < 2)
        {
            lr.positionCount = 2;
            lr.SetPosition(0, a);
            lr.SetPosition(1, b);
            return;
        }

        // Quadratic Bézier with a sagging control point
        // Control point is mid-point pulled along sagDirection
        Vector3 mid = (a + b) * 0.5f;
        float dist = Vector3.Distance(a, b);
        Vector3 ctrl = mid + sagDirection.normalized * (sagAmount + dist * sagPerMeter);

        lr.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float t = i / (segments - 1f);
            // Quadratic Bézier: Lerp(Lerp(a, ctrl, t), Lerp(ctrl, b, t), t)
            Vector3 p0 = Vector3.Lerp(a, ctrl, t);
            Vector3 p1 = Vector3.Lerp(ctrl, b, t);
            Vector3 p = Vector3.Lerp(p0, p1, t);
            lr.SetPosition(i, p);
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!lr) lr = GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.useWorldSpace = true;
            lr.widthCurve = AnimationCurve.Constant(0, 1, width);
            if (color != null) lr.colorGradient = color;
        }
        segments = Mathf.Max(2, segments);
    }
#endif
}

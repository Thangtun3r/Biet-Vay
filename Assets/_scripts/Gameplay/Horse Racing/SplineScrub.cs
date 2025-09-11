using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace UnityEngine.Splines
{
    /// <summary>
    /// Scrub an object along a (multi-)spline path.
    /// NEW: After one full pass across ALL splines, loop only on a chosen spline (e.g., Spline #2 for racetrack).
    /// </summary>
    [AddComponentMenu("Splines/Spline Scrub (Once then Loop on One Spline)")]
    [ExecuteInEditMode]
    public class SplineScrub : SplineComponent
    {
        public enum AlignmentMode
        {
            [InspectorName("None")] None,
            [InspectorName("Spline Element")] SplineElement,
            [InspectorName("Spline Object")]  SplineObject,
        }

        [Header("Path")]
        [SerializeField, Tooltip("The target SplineContainer with both splines (index 0 = first pass, index 1 = loop).")]
        SplineContainer m_Target;

        [Header("Scrub")]
        [SerializeField, Tooltip("Progress can grow beyond 1.0. One 'unit' ~= one full pass across ALL splines (before special looping kicks in).")]
        float m_Progress = 0f;

        [SerializeField, Tooltip("Normalized 0–1 start offset along the whole multi-spline path.")]
        [Range(0f, 1f)]
        float m_StartOffset = 0f;

        [SerializeField, Tooltip("Extra normalized travel added when Progress goes from 0 to 1 across the FIRST pass.\n" +
                                 "0 = exactly one pass, 0.05 = one pass + 5% more, negative stops short.")]
        [Range(-1f, 1f)]
        float m_LoopOverflow = 0f;

        [NonSerialized] float m_StartOffsetT = 0f;
        [NonSerialized] float m_StartOffsetDistance = 0f;

        [Header("Alignment")]
        [SerializeField, Tooltip("How the object aligns as it moves along the spline.")]
        AlignmentMode m_AlignmentMode = AlignmentMode.SplineElement;

        [SerializeField, Tooltip("Which axis of the GameObject is treated as the forward axis.")]
        AlignAxis m_ObjectForwardAxis = AlignAxis.ZAxis;

        [SerializeField, Tooltip("Which axis of the GameObject is treated as the up axis.")]
        AlignAxis m_ObjectUpAxis = AlignAxis.YAxis;

        [Header("Once then Loop on One Spline")]
        [SerializeField, Tooltip("If true, do exactly one full pass across ALL splines, then loop ONLY on the chosen spline.")]
        bool m_OnceThenLoopOnOneSpline = true;

        [SerializeField, Tooltip("After the first full pass, loop on this spline index (0-based).")]
        int m_LoopSplineIndex = 1;

        float m_TotalLengthAll = -1f;           // length of all splines combined
        float[] m_SplineLengths = null;         // per-spline lengths
        float m_LoopSplineLength = 0f;          // chosen loop spline length
        float m_LoopSplineStartDistance = 0f;   // distance where the loop spline begins in the combined path

        SplinePath<Spline> m_SplinePath;

        // --- Public API ---
        public SplineContainer Container
        {
            get => m_Target;
            set
            {
                m_Target = value;
                if (enabled && m_Target != null && m_Target.Splines != null)
                {
                    for (int i = 0; i < m_Target.Splines.Count; i++)
                        OnSplineChange(m_Target.Splines[i], -1, SplineModification.Default);
                }
                RecalculatePath();
                UpdateTransform();
            }
        }

        /// <summary>
        /// Current progress. Can go beyond 1.0. Do NOT clamp externally.
        /// </summary>
        public float Progress
        {
            get => m_Progress;
            set { m_Progress = value; UpdateTransform(); }
        }

        public float StartOffset
        {
            get => m_StartOffset;
            set
            {
                m_StartOffset = Mathf.Clamp01(value);
                UpdateStartOffsetUnits();
                UpdateTransform();
            }
        }

        public float LoopOverflow
        {
            get => m_LoopOverflow;
            set { m_LoopOverflow = Mathf.Clamp(value, -1f, 1f); UpdateTransform(); }
        }

        public AlignmentMode Alignment
        {
            get => m_AlignmentMode;
            set { m_AlignmentMode = value; UpdateTransform(); }
        }

        public AlignAxis ObjectForwardAxis
        {
            get => m_ObjectForwardAxis;
            set => m_ObjectUpAxis = SetObjectAlignAxis(value, ref m_ObjectForwardAxis, m_ObjectUpAxis);
        }

        public AlignAxis ObjectUpAxis
        {
            get => m_ObjectUpAxis;
            set => m_ObjectForwardAxis = SetObjectAlignAxis(value, ref m_ObjectUpAxis, m_ObjectForwardAxis);
        }

        public event Action<Vector3, Quaternion> Updated;

        // --- Unity lifecycle ---
        void Awake()
        {
            RecalculatePath();
            UpdateTransform();
        }

        void OnEnable()
        {
            RecalculatePath();
            Spline.Changed += OnSplineChange;
            UpdateTransform();
        }

        void OnDisable()
        {
            Spline.Changed -= OnSplineChange;
        }

        void OnValidate()
        {
            m_StartOffset = Mathf.Clamp01(m_StartOffset);
            m_LoopOverflow = Mathf.Clamp(m_LoopOverflow, -1f, 1f);
            m_LoopSplineIndex = Mathf.Max(0, m_LoopSplineIndex);
            RecalculatePath();
            UpdateTransform();
        }

        void Update()
        {
            UpdateTransform();
        }

        // --- Core calc ---
        void RecalculatePath()
        {
            if (m_Target != null)
            {
                m_SplinePath = new SplinePath<Spline>(m_Target.Splines);
                m_TotalLengthAll = (m_SplinePath != null) ? m_SplinePath.GetLength() : 0f;

                // Per-spline lengths + where the loop spline starts
                int count = (m_Target.Splines != null) ? m_Target.Splines.Count : 0;
                m_SplineLengths = (count > 0) ? new float[count] : Array.Empty<float>();
                float acc = 0f;
                for (int i = 0; i < count; i++)
                {
                    // Prefer the API available in your package. If GetLength() exists on Spline, use it.
                    // Fallback to SplineUtility.CalculateLength.
                    float len;
                    if (m_Target.Splines[i] != null)
                    {
                        // If Spline has GetLength():
                        try
                        {
                            len = m_Target.Splines[i].GetLength();
                        }
                        catch
                        {
                            // Otherwise, use CalculateLength with the container’s localToWorld
                            len = SplineUtility.CalculateLength(m_Target.Splines[i], m_Target.transform.localToWorldMatrix);
                        }
                    }
                    else
                    {
                        len = 0f;
                    }



                    m_SplineLengths[i] = len;
                }

                // Clamp chosen loop spline
                if (count > 0)
                    m_LoopSplineIndex = Mathf.Clamp(m_LoopSplineIndex, 0, count - 1);
                else
                    m_LoopSplineIndex = 0;

                // Compute loop spline start distance and length
                m_LoopSplineStartDistance = 0f;
                for (int i = 0; i < m_LoopSplineIndex && i < m_SplineLengths.Length; i++)
                    m_LoopSplineStartDistance += m_SplineLengths[i];
                m_LoopSplineLength = (m_SplineLengths.Length > 0 && m_LoopSplineIndex < m_SplineLengths.Length)
                    ? m_SplineLengths[m_LoopSplineIndex]
                    : 0f;

                UpdateStartOffsetUnits();
            }
            else
            {
                m_SplinePath = null;
                m_TotalLengthAll = -1f;
                m_SplineLengths = null;
                m_LoopSplineLength = 0f;
                m_LoopSplineStartDistance = 0f;
                m_StartOffsetT = 0f;
                m_StartOffsetDistance = 0f;
            }
        }

        void UpdateStartOffsetUnits()
        {
            if (m_SplinePath != null && m_TotalLengthAll > 0f)
            {
                m_StartOffsetDistance = m_StartOffset * m_TotalLengthAll;
                m_StartOffsetT = m_SplinePath.ConvertIndexUnit(m_StartOffsetDistance, PathIndexUnit.Distance, PathIndexUnit.Normalized);
            }
            else
            {
                m_StartOffsetT = 0f;
                m_StartOffsetDistance = 0f;
            }
        }

        float GetTWithOffset()
        {
            if (m_Target == null || m_SplinePath == null || m_TotalLengthAll <= 0f)
                return 0f;

            // Interpret Progress in units of "full-path passes"
            var turns = 1f + m_LoopOverflow;
            float unwrappedDistance = m_StartOffsetDistance + (m_Progress * turns * m_TotalLengthAll);

            if (!m_OnceThenLoopOnOneSpline)
            {
                // Original behavior: always wrap on the whole combined path
                float tAll = (unwrappedDistance / m_TotalLengthAll);
                return Mathf.Repeat(tAll, 1f);
            }

            // First full pass across ALL splines?
            if (unwrappedDistance <= m_TotalLengthAll)
            {
                float tFirst = unwrappedDistance / m_TotalLengthAll;
                return Mathf.Clamp01(tFirst);
            }

            // Past the first pass: loop ONLY inside the chosen spline's distance range
            if (m_LoopSplineLength <= 0f)
                return 0f;

            float remainder = unwrappedDistance - m_TotalLengthAll;
            float distanceOnLoopSpline = m_LoopSplineStartDistance + Mathf.Repeat(remainder, m_LoopSplineLength);

            // Map absolute distance back to normalized 0..1 over the combined path
            float tCombined = distanceOnLoopSpline / m_TotalLengthAll;
            return Mathf.Repeat(tCombined, 1f);
        }

        void UpdateTransform()
        {
            if (m_Target == null || m_SplinePath == null)
                return;

            var t = GetTWithOffset();
            var position = m_Target.EvaluatePosition(m_SplinePath, t);
            var rotation = ComputeRotation(t);

#if UNITY_EDITOR
            if (EditorApplication.isPlaying || !Application.isPlaying)
            {
                transform.position = position;
                if (m_AlignmentMode != AlignmentMode.None)
                    transform.rotation = rotation;
            }
#else
            transform.position = position;
            if (m_AlignmentMode != AlignmentMode.None)
                transform.rotation = rotation;
#endif
            Updated?.Invoke(position, rotation);
        }

        Quaternion ComputeRotation(float t)
        {
            var remappedForward = GetAxis(m_ObjectForwardAxis);
            var remappedUp = GetAxis(m_ObjectUpAxis);
            var axisRemapRotation = Quaternion.Inverse(Quaternion.LookRotation(remappedForward, remappedUp));

            if (m_AlignmentMode == AlignmentMode.None)
                return transform.rotation;

            Vector3 forward = Vector3.forward;
            Vector3 up = Vector3.up;

            switch (m_AlignmentMode)
            {
                case AlignmentMode.SplineElement:
                    forward = m_Target.EvaluateTangent(m_SplinePath, t);
                    if (forward.sqrMagnitude <= Mathf.Epsilon)
                    {
                        forward = t < 1f
                            ? m_Target.EvaluateTangent(m_SplinePath, Mathf.Min(1f, t + 0.01f))
                            : m_Target.EvaluateTangent(m_SplinePath, t - 0.01f);
                    }
                    forward.Normalize();
                    up = m_Target.EvaluateUpVector(m_SplinePath, t);
                    break;

                case AlignmentMode.SplineObject:
                    var objectRotation = m_Target.transform.rotation;
                    forward = objectRotation * forward;
                    up = objectRotation * up;
                    break;
            }

            return Quaternion.LookRotation(forward, up) * axisRemapRotation;
        }

        AlignAxis SetObjectAlignAxis(AlignAxis newValue, ref AlignAxis targetAxis, AlignAxis otherAxis)
        {
            if (newValue == otherAxis)
            {
                otherAxis = targetAxis;
                targetAxis = newValue;
            }
            else if ((int)newValue % 3 != (int)otherAxis % 3)
            {
                targetAxis = newValue;
            }

            UpdateTransform();
            return otherAxis;
        }

        void OnSplineChange(Spline spline, int knotIndex, SplineModification modificationType)
        {
            RecalculatePath();
            UpdateTransform();
        }
    }
}

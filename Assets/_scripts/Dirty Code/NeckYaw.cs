using UnityEngine;

/// Rotates a neck bone around its local Y (green) axis only.
/// Optionally adds an offset read from a tagged object.
/// - Assign this to your character root (or anywhere).
public class NeckYawOnly : MonoBehaviour
{
    [Header("Bone")]
    [Tooltip("Neck bone to rotate (local Y axis only).")]
    public Transform neckBone;

    [Header("Control")]
    [Tooltip("Base yaw in degrees to apply each frame (e.g., from input/aim).")]
    public float baseYawDegrees = 0f;

    [Tooltip("Clamp absolute yaw (degrees). Set to 0 to disable.")]
    public float maxAbsYaw = 80f;

    [Header("Offset via Tag")]
    [Tooltip("If set, we’ll try to find a GameObject with this tag for yaw offset.")]
    public string offsetTag = "";
    [Tooltip("If true, offset comes from the tagged object's local Y rotation.\n" +
             "If false, we'll try to read a float from a component implementing IYawOffset on that object.")]
    public bool useTaggedObjectYRotation = true;

    [Tooltip("Optional smoothing (degrees/sec). Set 0 for no smoothing.")]
    public float yawSlewSpeed = 720f;

    // --- Internals ---
    Quaternion _neckBaseRot;
    float _currentYaw;
    IYawOffset _offsetProvider;
    Transform _offsetTransform;

    void Awake()
    {
        if (neckBone == null)
        {
            Debug.LogError($"{nameof(NeckYawOnly)}: Neck bone not assigned.", this);
            enabled = false;
            return;
        }

        _neckBaseRot = neckBone.localRotation;

        if (!string.IsNullOrEmpty(offsetTag))
        {
            var tagged = GameObject.FindGameObjectWithTag(offsetTag);
            if (tagged != null)
            {
                if (useTaggedObjectYRotation)
                {
                    _offsetTransform = tagged.transform;
                }
                else
                {
                    _offsetProvider = tagged.GetComponent<IYawOffset>();
                    if (_offsetProvider == null)
                        Debug.LogWarning($"{nameof(NeckYawOnly)}: No IYawOffset found on tagged object '{offsetTag}'. Falling back to 0 offset.", this);
                }
            }
            else
            {
                Debug.LogWarning($"{nameof(NeckYawOnly)}: No object found with tag '{offsetTag}'.", this);
            }
        }
    }

    void LateUpdate()
    {
        // 1) Get offset from tag (if any)
        float offsetDeg = 0f;
        if (_offsetTransform != null && useTaggedObjectYRotation)
        {
            offsetDeg = _offsetTransform.localEulerAngles.y;
            // Convert 0..360 to -180..180 for nicer math:
            if (offsetDeg > 180f) offsetDeg -= 360f;
        }
        else if (_offsetProvider != null)
        {
            offsetDeg = _offsetProvider.GetYawOffsetDegrees();
        }

        // 2) Combine base yaw + offset and clamp
        float targetYaw = baseYawDegrees + offsetDeg;
        if (maxAbsYaw > 0f) targetYaw = Mathf.Clamp(targetYaw, -maxAbsYaw, maxAbsYaw);

        // 3) Optional smoothing
        if (yawSlewSpeed > 0f)
        {
            _currentYaw = Mathf.MoveTowardsAngle(_currentYaw, targetYaw, yawSlewSpeed * Time.deltaTime);
        }
        else
        {
            _currentYaw = targetYaw;
        }

        // 4) Apply ONLY around local Y (green) axis, preserving original X/Z from bind pose
        neckBone.localRotation = _neckBaseRot * Quaternion.AngleAxis(_currentYaw, Vector3.up);
    }
}

/// Optional interface if you want to supply the yaw offset from a script.
/// Attach any component that implements this to the tagged object.
public interface IYawOffset
{
    float GetYawOffsetDegrees();
}

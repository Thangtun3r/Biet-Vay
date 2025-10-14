using UnityEngine;
using Cinemachine;

public class HoldToZoom : MonoBehaviour
{
    [Header("References")]
    public CinemachineVirtualCamera vcam;

    [Header("Zoom Settings")]
    public float zoomedFOV = 30f;
    public float normalFOV = 60f;
    public float smooth = 10f;

    void LateUpdate()
    {
        if (vcam == null) return;

        float targetFOV = Input.GetMouseButton(1) ? zoomedFOV : normalFOV;

        var lens = vcam.m_Lens;
        lens.FieldOfView = Mathf.MoveTowards(lens.FieldOfView, targetFOV, smooth * Time.deltaTime);
        vcam.m_Lens = lens;
    }
}
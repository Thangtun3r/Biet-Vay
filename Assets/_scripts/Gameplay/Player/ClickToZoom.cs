/*using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineBrain))]
public class HoldToZoom : MonoBehaviour
{
    CinemachineBrain brain;
    public float zoomedFOV = 30f;
    public float normalFOV = 60f;
    public float smooth = 10f;

    void Awake() => brain = GetComponent<CinemachineBrain>();

    void LateUpdate()
    {
        if (brain == null || brain.IsBlending) return; // <-- let blends happen

        var vcam = brain.ActiveVirtualCamera as CinemachineVirtualCamera;
        if (vcam == null) return;

        float targetFOV = Input.GetMouseButton(1) ? zoomedFOV : normalFOV;

        var lens = vcam.m_Lens;
        lens.FieldOfView = Mathf.MoveTowards(lens.FieldOfView, targetFOV, smooth * Time.deltaTime);
        vcam.m_Lens = lens;
    }
}*/
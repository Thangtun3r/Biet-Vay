using UnityEngine;
using Cinemachine;

public class HoldToZoom : MonoBehaviour
{
    public CinemachineBrain brain;
    public float zoomedFOV = 30f;
    public float normalFOV = 60f;
    public float smooth = 10f;

    void Update()
    {
        var activeCam = brain.ActiveVirtualCamera as CinemachineVirtualCamera;
        if (activeCam == null) return;

        float targetFOV = Input.GetMouseButton(1) ? zoomedFOV : normalFOV;

        var lens = activeCam.m_Lens;
        lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFOV, Time.deltaTime * smooth);
        activeCam.m_Lens = lens;
    }
}
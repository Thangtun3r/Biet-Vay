using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CineMachineManager : MonoBehaviour
{
    [Serializable]
    public class CineMachine
    {
        public string name;
        public int id;
        public CinemachineVirtualCamera vcam;
        [HideInInspector] public int basePriority;
    }

    [Header("Register your virtual cameras here")]
    public List<CineMachine> cameras = new List<CineMachine>();

    private readonly Dictionary<int, CineMachine> _byId = new Dictionary<int, CineMachine>();


    private void OnEnable()
    {
        GameManager.OnVMBoost += BoostById;
        GameManager.OnVMReset += ResetAllToBase;
    }

    private void OnDisable()
    {
        GameManager.OnVMBoost -= BoostById;
        GameManager.OnVMReset -= ResetAllToBase;
    }

    void Awake()
    {
        _byId.Clear();
        foreach (var cam in cameras)
        {
            if (cam == null || cam.vcam == null) continue;
            cam.basePriority = cam.vcam.Priority; // remember the base
            if (!_byId.ContainsKey(cam.id))
                _byId.Add(cam.id, cam);
        }
    }

    /// <summary>Raise a camera’s priority by +2.</summary>
    public void BoostById(int id)
    {
        if (_byId.TryGetValue(id, out var cam) && cam.vcam != null)
            cam.vcam.Priority = cam.basePriority + 2;
    }

    /// <summary>Reset one camera back to its base priority.</summary>
    public void ResetToBase(int id)
    {
        if (_byId.TryGetValue(id, out var cam) && cam.vcam != null)
            cam.vcam.Priority = cam.basePriority;
    }

    /// <summary>Reset all cameras back to their base priorities.</summary>
    public void ResetAllToBase()
    {
        foreach (var cam in cameras)
            if (cam?.vcam != null)
                cam.vcam.Priority = cam.basePriority;
    }
}
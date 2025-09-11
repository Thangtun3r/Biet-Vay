// HorseVisualManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class HorseVisualManager : MonoBehaviour
{
    [Header("Links")]
    [Tooltip("RaceManager that owns the logic horses.")]
    public RaceManager raceManager;

    [Tooltip("Prefab for the visual horse. It must have a SplineScrub (on root or child).")]
    public GameObject visualHorsePrefab;

    [Tooltip("Optional: The spline container used for the race track. If set, assigned to each SplineScrub.")]
    public SplineContainer trackContainer;

    [Tooltip("Optional parent for spawned visual horses. If null, uses this GameObject.")]
    public Transform visualsParent;

    private readonly List<GameObject> _visuals = new List<GameObject>();
    private bool _wired;

    private void OnEnable()
    {
        if (visualsParent == null) visualsParent = transform;
    }

    private void Start()
    {
        if (raceManager == null || visualHorsePrefab == null)
        {
            Debug.LogError("[HorseVisualManager] raceManager or visualHorsePrefab is not set.");
            return;
        }
        StartCoroutine(WireWhenReady());
    }

    private IEnumerator WireWhenReady()
    {
        // Wait until RaceManager has spawned Horse2D children
        Horse2D[] horses = null;
        while (horses == null || horses.Length == 0)
        {
            horses = raceManager.GetComponentsInChildren<Horse2D>(includeInactive: true);
            yield return null; // try again next frame
        }

        // Spawn visuals and pipe progress (progress-only; no layout)
        var horsesList = new List<Horse2D>(horses);
        HandleHorsesSpawned(horsesList);
        _wired = true;
    }

    private void HandleHorsesSpawned(IReadOnlyList<Horse2D> horses)
    {
        if (horses == null || horses.Count == 0) return;

        // (Optional) clear previously spawned visuals
        foreach (var go in _visuals) if (go) Destroy(go);
        _visuals.Clear();

        for (int i = 0; i < horses.Count; i++)
        {
            var logic = horses[i];
            var visGO = Instantiate(visualHorsePrefab, visualsParent);
            visGO.name = $"{logic.name} (VISUAL)";

            // Find SplineScrub on the prefab (root or any child)
            var scrub = visGO.GetComponentInChildren<SplineScrub>();
            if (scrub == null)
            {
                Debug.LogError($"[HorseVisualManager] Visual prefab '{visualHorsePrefab.name}' has no SplineScrub.");
                Destroy(visGO);
                continue;
            }

            if (trackContainer != null)
                scrub.Container = trackContainer;

            // Pipe progress only (no other visual logic)
            logic.ProgressChanged += p => scrub.Progress = p;

            // Initialize visual to current progress
            scrub.Progress = logic.progress01;

            _visuals.Add(visGO);
        }
    }
}

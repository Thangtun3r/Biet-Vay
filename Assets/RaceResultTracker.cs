using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RaceResultsTracker : MonoBehaviour
{
    [System.Serializable]
    public class HorseResult
    {
        public Horse2D horse;
        public int place;
        public float finishTime;
    }

    private RaceManager _raceManager;
    private readonly List<HorseResult> _results = new List<HorseResult>();
    private bool _raceRunning;
    private float _raceTime;

    public IReadOnlyList<HorseResult> Results => _results;

    public event System.Action<HorseResult> HorseFinished;
    public event System.Action RaceCompleted;

    private void Awake()
    {
        _raceManager = GetComponent<RaceManager>();
        if (_raceManager == null)
            Debug.LogError("[RaceResultsTracker] RaceManager not found on same GameObject.");
    }

    private void OnEnable()
    {
        if (_raceManager != null)
        {
            _raceManager.HorsesProgressUpdated += OnProgressUpdated;
            _raceManager.RaceStarted += OnRaceStarted;
        }
    }

    private void OnDisable()
    {
        if (_raceManager != null)
        {
            _raceManager.HorsesProgressUpdated -= OnProgressUpdated;
            _raceManager.RaceStarted -= OnRaceStarted;
        }
    }

    private void Update()
    {
        if (_raceRunning)
            _raceTime += Time.deltaTime;
    }

    private void OnRaceStarted()
    {
        _results.Clear();
        _raceTime = 0f;
        _raceRunning = true;
    }

    private void OnProgressUpdated(IReadOnlyList<Horse2D> horses)
    {
        if (!_raceRunning || horses == null || horses.Count == 0) return;

        for (int i = 0; i < horses.Count; i++)
        {
            var h = horses[i];
            if (h.progress01 >= 1f && !_results.Exists(r => r.horse == h))
            {
                var result = new HorseResult
                {
                    horse = h,
                    place = _results.Count + 1,
                    finishTime = _raceTime
                };
                _results.Add(result);
                HorseFinished?.Invoke(result);

                Debug.Log($"🏁 {h.name} finished place #{result.place} at {result.finishTime:0.00}s");
                h.enabled = false; // optional: stop movement

                if (_results.Count == horses.Count)
                {
                    _raceRunning = false;
                    RaceCompleted?.Invoke();
                }
            }
        }
    }
}

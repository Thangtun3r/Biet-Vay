using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RaceResultsTracker : MonoBehaviour
{
    [Serializable]
    public class HorseResult
    {
        public Horse2D horse;
        public int index;        // index in RaceManager.Horses (per-race ID)
        public int place;        // 1 = winner
        public float finishTime; // seconds since RaceStarted
    }

    private RaceManager _raceManager;
    private readonly List<HorseResult> _results = new List<HorseResult>();
    private bool _raceRunning;
    private float _raceTime;

    public IReadOnlyList<HorseResult> Results => _results;

    public event Action<HorseResult> HorseFinished;

    // Now this passes just the int ID (index) of the winner
    public static event Action<int> OnRaceCompleted;

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
                int idx = GetHorseIndex(h);
                if (idx < 0)
                {
          
                    continue;
                }

                var result = new HorseResult
                {
                    horse = h,
                    index = idx,
                    place = _results.Count + 1,
                    finishTime = _raceTime
                };

                _results.Add(result);
                HorseFinished?.Invoke(result);
                
                h.enabled = false; // optional: stop movement

                if (_results.Count == 1)
                {
                    _raceRunning = false;
                    OnRaceCompleted?.Invoke(_results[0].index);
                }
            }
        }
    }

    private int GetHorseIndex(Horse2D horse)
    {
        for (int i = 0; i < _raceManager.Horses.Count; i++)
        {
            if (_raceManager.Horses[i] == horse)
                return i;
        }
        return -1; // not found
    }
}

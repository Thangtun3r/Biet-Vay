using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FinishLineTrigger : MonoBehaviour
{
    // ── Public Events ───────────────────────────────────────────────
    // Winner: index in RaceManager.Horses (same per-race ID used elsewhere)
    public static event Action<int> OnWinnerDetermined;
    // Race completed: fired when all expected horses have finished (passes winner index)
    public static event Action<int> OnRaceCompleted;

    // ── Refs & State ────────────────────────────────────────────────
    private RaceManager raceManager;

    private int currentPlace = 0;       // increments as horses cross
    private float raceStartTime = 0f;   // set exactly on RaceStarted

    private int expectedFinishers = 0;  // counted once at race start
    private bool completionInvoked = false;
    private int winnerIndex = -1;

    private void Awake()
    {
        raceManager = GetComponentInParent<RaceManager>();
        if (raceManager == null)
            raceManager = FindObjectOfType<RaceManager>();
        if (raceManager == null)
            Debug.LogError("[FinishLineTrigger] RaceManager not found in scene.");
    }

    private void OnEnable()
    {
        if (raceManager != null)
            raceManager.RaceStarted += OnRaceStarted; // precise start after preRaceDelay
    }

    private void OnDisable()
    {
        if (raceManager != null)
            raceManager.RaceStarted -= OnRaceStarted;
    }

    // Called by RaceManager right when horses are enabled and race truly begins
    private void OnRaceStarted()
    {
        currentPlace = 0;
        winnerIndex = -1;
        completionInvoked = false;
        raceStartTime = Time.time;

        // Count active/enabled horses once to know how many should finish
        expectedFinishers = 0;
        if (raceManager != null && raceManager.Horses != null)
        {
            for (int i = 0; i < raceManager.Horses.Count; i++)
            {
                var h = raceManager.Horses[i];
                if (h != null && h.gameObject.activeInHierarchy && h.enabled)
                    expectedFinishers++;
            }
        }

        Debug.Log($"[FinishLineTrigger] Race STARTED · expectedFinishers={expectedFinishers}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Identify the horse that crossed (collider may be on a child)
        Horse2D horse = other.GetComponentInParent<Horse2D>();
        if (horse == null) return;

        currentPlace++;                               // 1 = winner
        float finishTime = Time.time - raceStartTime; // seconds since real start

        // Update that horse's visual
        HorseVisual visual = horse.GetComponent<HorseVisual>();
        if (visual != null)
            visual.ShowResult(finishTime, currentPlace);

        Debug.Log($"🏁 {horse.name} finished · place {currentPlace} · time {finishTime:0.00}s");

        // Winner: announce immediately on first finisher
        if (currentPlace == 1)
        {
            winnerIndex = GetHorseIndex(horse);
            OnWinnerDetermined?.Invoke(winnerIndex);
            OnRaceCompleted?.Invoke(winnerIndex);
        }

        // Completion: when everyone we expected has finished
        if (!completionInvoked && expectedFinishers > 0 && currentPlace >= expectedFinishers)
        {
            completionInvoked = true;
            if (winnerIndex < 0) winnerIndex = GetHorseIndex(horse); // fallback
            
            
        }
    }

    private int GetHorseIndex(Horse2D target)
    {
        if (target == null) return -1;

        // ✅ Use roster index if available (set by HorseRosterAssigner)
        if (target.horseIndex >= 0)
            return target.horseIndex;

        // ↩️ Fallback: current position in RaceManager.Horses
        if (raceManager != null && raceManager.Horses != null)
        {
            for (int i = 0; i < raceManager.Horses.Count; i++)
                if (raceManager.Horses[i] == target) return i;
        }
        return -1;
    }

}

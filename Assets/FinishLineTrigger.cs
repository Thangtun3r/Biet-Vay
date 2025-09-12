using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FinishLineTrigger : MonoBehaviour
{
    private int currentPlace = 0;
    private float raceStartTime;

    private void Start()
    {
        // Record race start time (or reset when RaceManager.RaceStarted fires)
        raceStartTime = Time.time;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Trigger entered by {other.name}");

        // Get Horse2D from parent (covers cases where collider is on a child)
        Horse2D horse = other.GetComponentInParent<Horse2D>();
        if (horse == null) return;

        currentPlace++; // update place count
        float finishTime = Time.time - raceStartTime;

        // Get HorseVisual from the same parent (where Horse2D lives)
        HorseVisual visual = horse.GetComponent<HorseVisual>();
        if (visual != null)
        {
            visual.ShowResult(finishTime, currentPlace);
        }

        Debug.Log($"🏁 {horse.name} finished at place {currentPlace} with time {finishTime:0.00}s");
    }
}
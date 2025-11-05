using System.Collections;
using UnityEngine;
using FMODUnity;

public class NPCCrowdController : MonoBehaviour
{
    public RaceManager raceManager;
    public GameObject NPCPreRound;
    public GameObject NPCDuringRound;

    [Header("FMOD Crowd Sound Settings")]
    [EventRef] public string crowdCheerEvent; // FMOD event path
    [EventRef] public string crowdWinEvent;   // FMOD event path for win
    [EventRef] public string crowdLoseEvent;  // FMOD event path for lose
    public float startDelay = 2f;             // Delay before first play
    public float interval = 5f;               // Time between plays

    private bool raceCompleted = false;
    private Coroutine crowdSoundRoutine;

    private void OnEnable()
    {
        BettingSlip.OnSlipEnabled += OnRaceStarted;
        FinishLineTrigger.OnRaceCompleted += OnRaceCompleted;

        // Subscribe to payout events
        PayoutManager.OnBettingWin += OnPayoutWon;
        PayoutManager.OnBettingLose += OnPayoutLost;
    }

    private void OnDisable()
    {
        BettingSlip.OnSlipEnabled -= OnRaceStarted;
        FinishLineTrigger.OnRaceCompleted -= OnRaceCompleted;

        // Unsubscribe from payout events
        PayoutManager.OnBettingWin -= OnPayoutWon;
        PayoutManager.OnBettingLose -= OnPayoutLost;
    }

    private void OnRaceStarted()
    {
        NPCDuringRound.SetActive(true);
        NPCPreRound.SetActive(false);

        raceCompleted = false;

        // Start the FMOD one-shot coroutine
        if (crowdSoundRoutine != null)
            StopCoroutine(crowdSoundRoutine);

        crowdSoundRoutine = StartCoroutine(PlayCrowdSounds());
    }

    private void OnRaceCompleted(int winnerIndex)
    {
        raceCompleted = true;

        // Stop the coroutine if it's still running
        if (crowdSoundRoutine != null)
        {
            StopCoroutine(crowdSoundRoutine);
            crowdSoundRoutine = null;
        }
    }

    private IEnumerator PlayCrowdSounds()
    {
        // Initial delay before first sound
        yield return new WaitForSeconds(startDelay);

        // Loop as long as race is active
        while (!raceCompleted)
        {
            if (!string.IsNullOrEmpty(crowdCheerEvent))
            {
                RuntimeManager.PlayOneShot(crowdCheerEvent, transform.position);
            }

            yield return new WaitForSeconds(interval);
        }
    }

    // Handlers for payout events
    private void OnPayoutWon()
    {
        if (!string.IsNullOrEmpty(crowdWinEvent))
        {
            RuntimeManager.PlayOneShot(crowdWinEvent, transform.position);
        }
    }

    private void OnPayoutLost()
    {
        if (!string.IsNullOrEmpty(crowdLoseEvent))
        {
            RuntimeManager.PlayOneShot(crowdLoseEvent, transform.position);
        }
    }
}
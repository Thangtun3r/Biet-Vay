using System;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class PayoutManager : MonoBehaviour
{
    [SerializeField] public DialogueRunner dialogueRunner;

    private RaceManager raceManager;

    // Track the player's current bet
    private int placedHorseIndex = -1;
    private float placedAmount = 0f;

    // Cache odds by horse index (from RaceManager.OddsComputed)
    private readonly Dictionary<int, RaceManager.OddsEntry> oddsByIndex = new Dictionary<int, RaceManager.OddsEntry>();

    // Winner (per-race ID / index in RaceManager.Horses)
    private int winningHorseIndex = -1;

    private void Awake()
    {
        raceManager = GetComponent<RaceManager>();
        if (dialogueRunner == null)
            Debug.LogWarning("[PayoutManager] DialogueRunner reference not set.");
    }

    private void OnEnable()
    {
        if (raceManager != null)
            raceManager.OddsComputed += DisplayOdds;

        GameManager.OnBetPlaced += HandleBetPlaced;
        GameManager.OnPayout += HandlePayout;

        // NOTE: RaceResultsTracker.OnRaceCompleted must be: public static event Action<int>
        RaceResultsTracker.OnRaceCompleted += HandleRaceCompleted;
    }

    private void OnDisable()
    {
        if (raceManager != null)
            raceManager.OddsComputed -= DisplayOdds;

        GameManager.OnBetPlaced -= HandleBetPlaced;
        GameManager.OnPayout -= HandlePayout;

        RaceResultsTracker.OnRaceCompleted -= HandleRaceCompleted;
    }

    // Called when RaceResultsTracker signals race end with winnerId (index in RaceManager.Horses)
    private void HandleRaceCompleted(int winnerId)
    {
        winningHorseIndex = winnerId;

        dialogueRunner.VariableStorage.SetValue("$raceCompleted", true);
        bool test = dialogueRunner.VariableStorage.TryGetValue("$raceCompleted", out bool val);

    }

    // Cache & (optionally) log odds as they’re computed
    private void DisplayOdds(IReadOnlyList<RaceManager.OddsEntry> oddsList)
    {
        oddsByIndex.Clear();
        foreach (var entry in oddsList)
        {
            oddsByIndex[entry.index] = entry;
            Debug.Log(
                $"Horse {entry.index} ({entry.horse.name}) → " +
                $"Prob: {entry.probability:P1}, Decimal: {entry.decimalOdds:0.00}, Fractional: {entry.fractional}"
            );
        }
    }

    private void HandleBetPlaced(int horseIndex)
    {
        placedHorseIndex = horseIndex;
        dialogueRunner.VariableStorage.TryGetValue("$currentHolding", out placedAmount);
        dialogueRunner.VariableStorage.SetValue("$currentHolding", 0);
    }

    private void HandlePayout()
    {
        if (placedHorseIndex == -1 || placedAmount <= 0f)
        {
            return;
        }

        if (winningHorseIndex == -1)
        {
            return;
        }

        if (!oddsByIndex.TryGetValue(placedHorseIndex, out var placedOdds))
        {
            return;
        }

        if (winningHorseIndex == placedHorseIndex)
        {
            dialogueRunner.VariableStorage.SetValue("$winningBet", true);
            float payout = placedAmount * placedOdds.decimalOdds;
            dialogueRunner.VariableStorage.SetValue("$currentHolding", payout);
        }
        else
        {
            dialogueRunner.VariableStorage.SetValue("$currentHolding", 0);
            dialogueRunner.VariableStorage.SetValue("$winningBet", false);
        }

        // Reset bet for next round
        placedHorseIndex = -1;
        placedAmount = 0f;
        winningHorseIndex = -1;
        oddsByIndex.Clear();
    }
}

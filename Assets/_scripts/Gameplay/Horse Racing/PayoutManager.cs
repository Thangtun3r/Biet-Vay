using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Yarn.Unity;
    
    public class PayoutManager : MonoBehaviour
    {
        [SerializeField] public DialogueRunner dialogueRunner;
    
        // Static events for win/lose so other code can subscribe to PayoutManager.OnBettingWin / OnBettingLose
        public static event Action OnBettingWin;
        public static event Action OnBettingLose;
    
        private RaceManager raceManager;
    
        // Track the player's current bet
        public int placedHorseIndex = -1;
        public float placedAmount = 0f;
    
        // Cache odds by horse index (from RaceManager.OddsComputed)
        private readonly Dictionary<int, RaceManager.OddsEntry> oddsByIndex = new Dictionary<int, RaceManager.OddsEntry>();
    
        // Winner (per-race ID / index in RaceManager.Horses)
        public int winningHorseIndex = -1;
    
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
            FinishLineTrigger.OnRaceCompleted += HandleRaceCompleted;
        }
    
        private void OnDisable()
        {
            if (raceManager != null)
                raceManager.OddsComputed -= DisplayOdds;
    
            GameManager.OnBetPlaced -= HandleBetPlaced;
            GameManager.OnPayout -= HandlePayout;
            FinishLineTrigger.OnRaceCompleted -= HandleRaceCompleted;
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
            if (placedHorseIndex == -1)
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
                Debug.Log("Player won the bet!");
                dialogueRunner.VariableStorage.SetValue("$winningBet", true);
                float payout = placedAmount * placedOdds.decimalOdds;
                dialogueRunner.VariableStorage.SetValue("$currentHolding", payout);
    
                // Invoke local static win event
                OnBettingWin?.Invoke();
            }
            else
            {
                dialogueRunner.VariableStorage.SetValue("$currentHolding", 0);
                dialogueRunner.VariableStorage.SetValue("$winningBet", false);
    
                // Invoke local static lose event
                OnBettingLose?.Invoke();
            }
    
            // Reset bet for next round
            placedHorseIndex = -1;
            placedAmount = 0f;
            winningHorseIndex = -1;
            oddsByIndex.Clear();
        }
    }
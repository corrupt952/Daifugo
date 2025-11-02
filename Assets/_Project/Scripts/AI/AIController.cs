using System.Collections;
using Daifugo.Core;
using Daifugo.Data;
using Daifugo.Events;
using Tang3cko.EventChannels;
using UnityEngine;

namespace Daifugo.AI
{
    /// <summary>
    /// Controls AI player turn execution
    /// Listens to turn changes and executes AI decisions with delay
    /// </summary>
    public class AIController : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Delay before AI takes action (simulates thinking time)")]
        [SerializeField] private float aiTurnDelay = 1.5f;

        [Header("Data")]
        [Tooltip("Player hands array (0 = Human, 1-3 = AI)")]
        [SerializeField] private PlayerHandSO[] playerHands;

        [Tooltip("Game rules configuration")]
        [SerializeField] private GameRulesSO gameRules;

        [Header("Events - Subscribe")]
        [Tooltip("Raised when game starts")]
        [SerializeField] private VoidEventChannelSO onGameStarted;

        [Tooltip("Raised when turn changes to a new player")]
        [SerializeField] private IntEventChannelSO onTurnChanged;

        [Tooltip("Raised when a card is played")]
        [SerializeField] private CardPlayedEventChannelSO onCardPlayed;

        [Tooltip("Raised when field is reset")]
        [SerializeField] private VoidEventChannelSO onFieldReset;

        [Tooltip("Raised when game ends")]
        [SerializeField] private IntEventChannelSO onGameEnded;

        [Header("Events - Raise")]
        [Tooltip("Raised when AI wants to play cards (supports 1 or more cards)")]
        [SerializeField] private ListCardEventChannelSO onPlayCardsRequested;

        [Tooltip("Raised when AI wants to pass")]
        [SerializeField] private VoidEventChannelSO onPassButtonClicked;

        // Runtime state (tracks field state from events)
        private FieldState currentFieldState;
        private bool isGameActive;

        // Services (pure C# classes - testable)
        private AIPlayerStrategy aiStrategy;

        private void Awake()
        {
            // Initialize pure C# AI strategy
            aiStrategy = new AIPlayerStrategy(gameRules);
        }

        private void OnEnable()
        {
            // Subscribe to notification events
            onGameStarted.OnEventRaised += HandleGameStarted;
            onTurnChanged.OnEventRaised += HandleTurnChanged;
            onCardPlayed.OnEventRaised += HandleCardPlayed;
            onFieldReset.OnEventRaised += HandleFieldReset;
            onGameEnded.OnEventRaised += HandleGameEnded;
        }

        private void OnDisable()
        {
            // Unsubscribe from notification events
            onGameStarted.OnEventRaised -= HandleGameStarted;
            onTurnChanged.OnEventRaised -= HandleTurnChanged;
            onCardPlayed.OnEventRaised -= HandleCardPlayed;
            onFieldReset.OnEventRaised -= HandleFieldReset;
            onGameEnded.OnEventRaised -= HandleGameEnded;
        }

        /// <summary>
        /// Handles game started event
        /// </summary>
        private void HandleGameStarted()
        {
            isGameActive = true;
            currentFieldState = FieldState.Empty();
        }

        /// <summary>
        /// Handles turn changed event - triggers AI execution for AI players
        /// </summary>
        /// <param name="playerID">New current player ID</param>
        private void HandleTurnChanged(int playerID)
        {
            // Only execute AI for players 1-3 (not player 0 = human)
            if (playerID >= 1 && playerID <= 3)
            {
                StartCoroutine(ExecuteAITurn(playerID));
            }
        }

        /// <summary>
        /// Handles card played event - updates tracked field state
        /// </summary>
        /// <param name="eventData">Card play event data</param>
        private void HandleCardPlayed(CardPlayedEventData eventData)
        {
            // Track the current field state
            currentFieldState = eventData.FieldState;
        }

        /// <summary>
        /// Handles field reset event - clears tracked field state
        /// </summary>
        private void HandleFieldReset()
        {
            currentFieldState = FieldState.Empty();
        }

        /// <summary>
        /// Handles game ended event
        /// </summary>
        /// <param name="winnerID">ID of winning player</param>
        private void HandleGameEnded(int winnerID)
        {
            isGameActive = false;
        }

        /// <summary>
        /// Executes AI turn with delay
        /// Supports both single and multiple card decisions
        /// </summary>
        /// <param name="aiPlayerID">AI player ID (1-3)</param>
        private IEnumerator ExecuteAITurn(int aiPlayerID)
        {
            // Wait for thinking delay
            yield return new WaitForSeconds(aiTurnDelay);

            // If game ended during delay, abort
            if (!isGameActive)
            {
                yield break;
            }

            // Get AI player's hand
            PlayerHandSO aiHand = playerHands[aiPlayerID];

            // Try multiple card decision first
            var multipleCards = aiStrategy.DecideMultipleCardAction(aiHand, currentFieldState);
            if (multipleCards != null && multipleCards.Count > 0)
            {
                onPlayCardsRequested.RaiseEvent(multipleCards);
                yield break;
            }

            // If field requires multiple cards but AI has no valid combination, pass
            if (!currentFieldState.IsEmpty && currentFieldState.GetLastPlayCount() > 1)
            {
                Debug.Log($"[AIController] AI Player {aiPlayerID} cannot match field requirement ({currentFieldState.GetLastPlayCount()} cards). Passing turn.");
                onPassButtonClicked.RaiseEvent();
                yield break;
            }

            // Single card decision (only when field is empty or requires single card)
            CardSO cardToPlay = aiStrategy.DecideAction(aiHand, currentFieldState);
            if (cardToPlay != null)
            {
                onPlayCardsRequested.RaiseEvent(new System.Collections.Generic.List<CardSO> { cardToPlay });
            }
            else
            {
                Debug.Log($"[AIController] AI Player {aiPlayerID} has no playable cards. Passing turn.");
                onPassButtonClicked.RaiseEvent();
            }
        }
    }
}

using Daifugo.Data;
using Daifugo.Events;
using Tang3cko.EventChannels;
using UnityEngine;

namespace Daifugo.Core
{
    /// <summary>
    /// Main game controller for Daifugo Phase 1
    /// Manages game state and coordinates between components via events
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Data")]
        [Tooltip("Deck containing all 52 cards")]
        [SerializeField] private DeckSO deck;

        [Tooltip("Player hands (0 = Human, 1-3 = AI)")]
        [SerializeField] private PlayerHandSO[] playerHands; // 4 elements

        [Header("Events - Commands")]
        [Tooltip("Raised when UI/AI requests to play a card")]
        [SerializeField] private CardEventChannelSO onPlayCardRequested;

        [Tooltip("Raised when Pass button is clicked")]
        [SerializeField] private VoidEventChannelSO onPassButtonClicked;

        [Header("Events - Notifications")]
        [Tooltip("Raised when game starts")]
        [SerializeField] private VoidEventChannelSO onGameStarted;

        [Tooltip("Raised when a card is successfully played")]
        [SerializeField] private CardPlayedEventChannelSO onCardPlayed;

        [Tooltip("Raised when field is reset (from TurnManager)")]
        [SerializeField] private VoidEventChannelSO onFieldReset;

        [Tooltip("Raised when game ends with winner ID")]
        [SerializeField] private IntEventChannelSO onGameEnded;

        [Header("Components")]
        [Tooltip("Turn manager component")]
        [SerializeField] private TurnManager turnManager;

        [Tooltip("Rule validator component")]
        [SerializeField] private RuleValidator ruleValidator;

        // Runtime state (no Variables - see C-008)
        private CardSO currentFieldCard;
        private bool isGameActive;

        private void OnEnable()
        {
            // Subscribe to command events
            onPlayCardRequested.OnEventRaised += HandlePlayCardRequested;
            onPassButtonClicked.OnEventRaised += HandlePassButtonClicked;
            onFieldReset.OnEventRaised += HandleFieldReset;
        }

        private void OnDisable()
        {
            // Unsubscribe from command events
            onPlayCardRequested.OnEventRaised -= HandlePlayCardRequested;
            onPassButtonClicked.OnEventRaised -= HandlePassButtonClicked;
            onFieldReset.OnEventRaised -= HandleFieldReset;
        }

        private void Start()
        {
            StartGame();
        }

        /// <summary>
        /// Initializes and starts a new game
        /// </summary>
        public void StartGame()
        {
            // 1. Reset game state
            currentFieldCard = null;
            isGameActive = true;

            // 2. Clear all hands
            foreach (var hand in playerHands)
            {
                if (hand != null)
                {
                    hand.Initialize();
                }
            }

            // 3. Initialize and shuffle deck
            if (deck != null)
            {
                deck.Initialize();

                // Validation check - if deck initialization failed, abort
                if (deck.CurrentDeck.Count == 0)
                {
                    Debug.LogError("[GameManager] Deck initialization failed.", this);
                    return;
                }

                deck.DistributeCards(playerHands);
            }
            else
            {
                Debug.LogError("[GameManager] Deck is not assigned.", this);
                return;
            }

            // 4. Sort all hands by strength (weak to strong)
            foreach (var hand in playerHands)
            {
                if (hand != null)
                {
                    hand.SortByStrength();
                }
            }

            // 5. Raise game started event FIRST (so UI can initialize)
            if (onGameStarted != null)
            {
                onGameStarted.RaiseEvent();
            }

            // 6. Initialize turn manager (which raises OnTurnChanged event)
            if (turnManager != null)
            {
                turnManager.Initialize();
            }
            else
            {
                Debug.LogError("[GameManager] TurnManager is not assigned.", this);
            }

            Debug.Log("[GameManager] Game started. Cards distributed to 4 players.");
        }

        /// <summary>
        /// Handles card play request from UI or AI (Command Event)
        /// Validates the play, executes it, and raises notification events
        /// </summary>
        private void HandlePlayCardRequested(CardSO card)
        {
            if (!isGameActive)
            {
                Debug.LogWarning("[GameManager] Card play requested while game is not active.");
                return;
            }

            // Get current player from TurnManager
            int currentPlayer = turnManager.GetCurrentPlayer();
            PlayerHandSO hand = playerHands[currentPlayer];

            // 1. Validation - check if card is in current player's hand
            if (!ruleValidator.IsCardInHand(card, hand))
            {
                Debug.LogWarning($"[GameManager] Card {card.name} is not in player {currentPlayer}'s hand.");
                return;
            }

            // 2. Validation - check if card can be played on current field
            if (!ruleValidator.CanPlayCard(card, currentFieldCard))
            {
                Debug.LogWarning($"[GameManager] Cannot play {card.name} on {currentFieldCard?.name ?? "empty field"}.");
                return;
            }

            // 3. Execute card play
            hand.RemoveCard(card);
            currentFieldCard = card;

            Debug.Log($"[GameManager] Player {currentPlayer} played {card.CardSuit} {card.Rank}");

            // 4. Raise notification event (for UI and other observers)
            var playedData = new CardPlayedEventData
            {
                Card = card,
                PlayerID = currentPlayer,
                FieldCard = currentFieldCard
            };
            onCardPlayed.RaiseEvent(playedData);

            // 5. Check win condition
            if (hand.IsEmpty)
            {
                EndGame(currentPlayer);
                return;
            }

            // 6. Advance turn (notify TurnManager)
            turnManager.OnCardPlayed(currentPlayer);
        }

        /// <summary>
        /// Handles pass event from UI or AI (Command Event)
        /// Any player can pass when they have no playable cards
        /// </summary>
        private void HandlePassButtonClicked()
        {
            if (!isGameActive)
            {
                Debug.LogWarning("[GameManager] Pass requested while game is not active.");
                return;
            }

            int currentPlayer = turnManager.GetCurrentPlayer();
            Debug.Log($"[GameManager] Player {currentPlayer} passed.");

            // Notify turn manager to advance turn
            turnManager.OnPlayerPass();
        }

        /// <summary>
        /// Handles field reset event from TurnManager (Notification Event)
        /// Clears the current field card so any card can be played
        /// </summary>
        private void HandleFieldReset()
        {
            currentFieldCard = null;
            Debug.Log("[GameManager] Field reset - any card can now be played.");
        }

        /// <summary>
        /// Ends the game with a winner
        /// </summary>
        private void EndGame(int winnerID)
        {
            isGameActive = false;

            Debug.Log($"[GameManager] Game ended. Player {winnerID} wins!");

            // Raise game ended notification event
            if (onGameEnded != null)
            {
                onGameEnded.RaiseEvent(winnerID);
            }
        }
    }
}

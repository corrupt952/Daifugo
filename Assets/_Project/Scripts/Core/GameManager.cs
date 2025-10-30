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

        [Tooltip("Game rules configuration")]
        [SerializeField] private GameRulesSO gameRules;

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

        // Runtime state (no Variables - see C-008)
        private CardSO currentFieldCard;
        private bool isGameActive;

        // Services (pure C# classes - testable)
        private GameLogic gameLogic;

        private void Awake()
        {
            // Initialize pure C# services
            gameLogic = new GameLogic(gameRules);
        }

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
        /// Uses GameLogic for testable card play logic with special rules (e.g., 8-cut)
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

            // Execute card play logic (testable)
            CardPlayResult result = gameLogic.PlayCard(card, hand, currentFieldCard);

            // Handle failure
            if (!result.IsSuccess)
            {
                Debug.LogWarning($"[GameManager] Card play failed: {result.ErrorMessage}");
                return;
            }

            // Update field card
            currentFieldCard = result.NewFieldCard;

            Debug.Log($"[GameManager] Player {currentPlayer} played {card.CardSuit} {card.Rank}");

            // Create callback for post-animation processing
            // This will be invoked by UI after card animation completes
            // Ensures game logic executes after visual feedback, maintaining proper game progression timeline
            System.Action onAnimationComplete = () =>
            {
                // Step 1: Check for special rules (8-cut)
                // Must execute before win check - even if last card is 8, field resets for potential continuation
                if (result.ShouldResetField)
                {
                    Debug.Log("[GameManager] 8-cut activated! Field will reset.");
                    currentFieldCard = null;
                    onFieldReset.RaiseEvent();
                }

                // Step 2: Check win condition
                // If player has no cards left, game ends immediately (no turn advance)
                if (result.IsWin)
                {
                    EndGame(currentPlayer);
                    return;
                }

                // Step 3: Advance turn based on result
                // TurnAdvanceType is derived from special rules:
                // - SamePlayer: 8-cut or other special rules that grant another turn
                // - NextPlayer: Normal card play
                turnManager.AdvanceTurn(currentPlayer, result.TurnAdvanceType);
            };

            // Raise notification event (for UI and other observers)
            var playedData = new CardPlayedEventData
            {
                Card = card,
                PlayerID = currentPlayer,
                FieldCard = currentFieldCard,
                OnAnimationComplete = onAnimationComplete
            };
            onCardPlayed.RaiseEvent(playedData);
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

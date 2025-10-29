using Daifugo.Data;
using Daifugo.Events;
using Tang3cko.EventChannels;
using UnityEngine;
using UnityEngine.UIElements;

namespace Daifugo.UI
{
    /// <summary>
    /// Main UI controller for the game screen
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class GameScreenUI : MonoBehaviour
    {
        [Header("Data")]
        [Tooltip("Player hands (0 = Human, 1-3 = AI)")]
        [SerializeField] private PlayerHandSO[] playerHands = new PlayerHandSO[4];

        [Header("Runtime State")]
        [Tooltip("Current player ID (managed via events)")]
        private int currentPlayerID = -1;

        [Tooltip("Is game currently active (managed via events)")]
        private bool isGameActive = false;

        [Header("Events - Subscribe (Notifications)")]
        [Tooltip("Raised when game starts")]
        [SerializeField] private VoidEventChannelSO onGameStarted;

        [Tooltip("Raised when turn changes")]
        [SerializeField] private IntEventChannelSO onTurnChanged;

        [Tooltip("Raised when a card is played (notification)")]
        [SerializeField] private CardPlayedEventChannelSO onCardPlayed;

        [Tooltip("Raised when field is reset")]
        [SerializeField] private VoidEventChannelSO onFieldReset;

        [Tooltip("Raised when game ends with winner ID")]
        [SerializeField] private IntEventChannelSO onGameEnded;

        [Header("Events - Raise (Commands)")]
        [Tooltip("Raised when UI requests to play a card")]
        [SerializeField] private CardEventChannelSO onPlayCardRequested;

        [Tooltip("Raised when Pass button is clicked")]
        [SerializeField] private VoidEventChannelSO onPassButtonClicked;

        // UI Elements
        private UIDocument uiDocument;
        private VisualElement root;
        private Label turnInfoText;
        private Button playCardButton;
        private Button passButton;
        private VisualElement gameEndScreen;
        private Label winnerText;
        private Button restartButton;
        private VisualElement playerHandContainer;
        private VisualElement fieldCardsContainer;

        // Hand UI components
        private HandUI playerHandUI;

        // Field cards
        private CardUI currentFieldCardUI;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            // Get root element
            root = uiDocument.rootVisualElement;

            // Query UI elements
            turnInfoText = root.Q<Label>("TurnInfoText");
            playCardButton = root.Q<Button>("PlayCardButton");
            passButton = root.Q<Button>("PassButton");
            gameEndScreen = root.Q<VisualElement>("GameEndScreen");
            winnerText = root.Q<Label>("WinnerText");
            restartButton = root.Q<Button>("RestartButton");
            playerHandContainer = root.Q<VisualElement>("PlayerHandContainer");
            fieldCardsContainer = root.Q<VisualElement>("FieldCardsContainer");

            // Register button callbacks
            if (playCardButton != null)
            {
                playCardButton.clicked += OnPlayCardButtonClick;
            }

            if (passButton != null)
            {
                passButton.clicked += OnPassButtonClick;
            }

            if (restartButton != null)
            {
                restartButton.clicked += OnRestartButtonClick;
            }

            // Subscribe to events
            if (onGameStarted != null)
            {
                onGameStarted.OnEventRaised += HandleGameStarted;
            }

            if (onTurnChanged != null)
            {
                onTurnChanged.OnEventRaised += HandleTurnChanged;
            }

            if (onGameEnded != null)
            {
                onGameEnded.OnEventRaised += HandleGameEnded;
            }

            if (onCardPlayed != null)
            {
                onCardPlayed.OnEventRaised += HandleCardPlayed;
            }

            if (onFieldReset != null)
            {
                onFieldReset.OnEventRaised += HandleFieldReset;
            }
        }

        private void OnDisable()
        {
            // Unregister button callbacks
            if (playCardButton != null)
            {
                playCardButton.clicked -= OnPlayCardButtonClick;
            }

            if (passButton != null)
            {
                passButton.clicked -= OnPassButtonClick;
            }

            if (restartButton != null)
            {
                restartButton.clicked -= OnRestartButtonClick;
            }

            // Unsubscribe from events
            if (onGameStarted != null)
            {
                onGameStarted.OnEventRaised -= HandleGameStarted;
            }

            if (onTurnChanged != null)
            {
                onTurnChanged.OnEventRaised -= HandleTurnChanged;
            }

            if (onGameEnded != null)
            {
                onGameEnded.OnEventRaised -= HandleGameEnded;
            }

            if (onCardPlayed != null)
            {
                onCardPlayed.OnEventRaised -= HandleCardPlayed;
            }

            if (onFieldReset != null)
            {
                onFieldReset.OnEventRaised -= HandleFieldReset;
            }
        }

        /// <summary>
        /// Handles game started event
        /// </summary>
        private void HandleGameStarted()
        {
            // Update runtime state
            isGameActive = true;

            // Clear field
            fieldCardsContainer?.Clear();
            currentFieldCardUI = null;

            // Initialize hand UI after cards are distributed
            if (playerHandContainer != null && playerHands.Length > 0 && playerHands[0] != null)
            {
                playerHandUI = new HandUI(playerHandContainer, playerHands[0]);

                // Subscribe to selection changed event
                playerHandUI.OnSelectionChanged += UpdateButtonStates;
            }

            // Hide game end screen
            gameEndScreen?.AddToClassList("game-end-screen--hidden");
        }

        /// <summary>
        /// Handles turn change event
        /// </summary>
        private void HandleTurnChanged(int newPlayerID)
        {
            // Update runtime state from event
            currentPlayerID = newPlayerID;

            UpdateTurnIndicator(newPlayerID);
            UpdateButtonStates();
            UpdatePlayableCardsHighlight();
        }

        /// <summary>
        /// Updates turn indicator text
        /// </summary>
        private void UpdateTurnIndicator(int playerID)
        {
            if (turnInfoText == null) return;

            if (playerID == 0)
            {
                turnInfoText.text = "Your Turn";
            }
            else
            {
                turnInfoText.text = $"CPU {playerID}'s Turn";
            }
        }

        /// <summary>
        /// Updates button states
        /// </summary>
        private void UpdateButtonStates()
        {
            if (passButton == null || playCardButton == null) return;

            bool isPlayerTurn = currentPlayerID == 0 && isGameActive;
            bool hasSelectedCards = playerHandUI != null && playerHandUI.GetSelectedCards().Count > 0;

            // Enable buttons only during player's turn
            passButton.SetEnabled(isPlayerTurn);

            // Play Card button enabled only if cards are selected
            playCardButton.SetEnabled(isPlayerTurn && hasSelectedCards);
        }

        /// <summary>
        /// Handles Play Card button click (raises command event)
        /// </summary>
        private void OnPlayCardButtonClick()
        {
            if (playerHandUI == null || onPlayCardRequested == null) return;

            var selectedCards = playerHandUI.GetSelectedCards();
            if (selectedCards.Count == 0) return;

            // Phase 1: Play single card only
            // Raise command event to request card play
            onPlayCardRequested.RaiseEvent(selectedCards[0]);

            // Clear selection after playing
            playerHandUI.ClearSelection();

            Debug.Log($"[GameScreenUI] Requested to play card: {selectedCards[0].CardSuit} {selectedCards[0].Rank}");
        }

        /// <summary>
        /// Handles Pass button click
        /// </summary>
        private void OnPassButtonClick()
        {
            if (onPassButtonClicked != null)
            {
                onPassButtonClicked.RaiseEvent();
            }

            // Clear selection when passing
            playerHandUI?.ClearSelection();
        }

        /// <summary>
        /// Handles game end event
        /// </summary>
        private void HandleGameEnded(int winnerID)
        {
            // Update runtime state
            isGameActive = false;

            ShowGameEndScreen(winnerID);
        }

        /// <summary>
        /// Shows game end screen with winner
        /// </summary>
        private void ShowGameEndScreen(int winnerID)
        {
            if (gameEndScreen == null || winnerText == null) return;

            // Show game end screen
            gameEndScreen.RemoveFromClassList("game-end-screen--hidden");

            // Update winner text
            if (winnerID == 0)
            {
                winnerText.text = "You Win!";
            }
            else
            {
                winnerText.text = $"CPU {winnerID} Wins!";
            }
        }

        /// <summary>
        /// Handles Restart button click
        /// </summary>
        private void OnRestartButtonClick()
        {
            // Hide game end screen
            if (gameEndScreen != null)
            {
                gameEndScreen.AddToClassList("game-end-screen--hidden");
            }

            // Restart game by reloading scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }

        /// <summary>
        /// Refreshes player hand UI
        /// </summary>
        public void RefreshPlayerHand()
        {
            playerHandUI?.Refresh();
        }

        /// <summary>
        /// Handles card played event (notification from GameManager)
        /// Refreshes UI after card is played
        /// </summary>
        private void HandleCardPlayed(CardPlayedEventData eventData)
        {
            // Refresh player hand only if it was the human player who played
            if (eventData.PlayerID == 0)
            {
                RefreshPlayerHand();
            }

            // Display card on field
            DisplayCardOnField(eventData.FieldCard);

            // Update playable cards highlight
            UpdatePlayableCardsHighlight();
        }

        /// <summary>
        /// Displays a card on the field
        /// </summary>
        private void DisplayCardOnField(CardSO card)
        {
            if (fieldCardsContainer == null || card == null) return;

            // Clear previous field card
            fieldCardsContainer.Clear();
            currentFieldCardUI = null;

            // Create new card UI for the field
            currentFieldCardUI = new CardUI(card);
            fieldCardsContainer.Add(currentFieldCardUI.Element);

            Debug.Log($"[GameScreenUI] Displayed {card.CardSuit} {card.Rank} on field");
        }

        /// <summary>
        /// Handles field reset event
        /// </summary>
        private void HandleFieldReset()
        {
            if (fieldCardsContainer == null) return;

            // Clear field
            fieldCardsContainer.Clear();
            currentFieldCardUI = null;

            Debug.Log("[GameScreenUI] Field cleared");

            // Update playable cards after field reset
            UpdatePlayableCardsHighlight();
        }

        /// <summary>
        /// Updates the highlight for playable cards
        /// </summary>
        private void UpdatePlayableCardsHighlight()
        {
            if (playerHandUI == null || playerHands == null || playerHands.Length == 0) return;

            // Only highlight during player's turn
            if (currentPlayerID != 0) return;

            // Get current field card strength
            int fieldStrength = 0;
            if (currentFieldCardUI != null && currentFieldCardUI.CardData != null)
            {
                fieldStrength = currentFieldCardUI.CardData.GetStrength();
            }

            // Get playable cards from player hand
            var playableCards = playerHands[0].GetPlayableCards(fieldStrength);

            // Highlight playable cards
            playerHandUI.HighlightPlayableCards(playableCards);
        }
    }
}

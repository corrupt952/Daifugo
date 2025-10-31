using Daifugo.Data;

namespace Daifugo.Core
{
    /// <summary>
    /// Turn advancement type for controlling turn progression
    /// </summary>
    public enum TurnAdvanceType
    {
        /// <summary>
        /// Normal card play or pass - advance to next player clockwise
        /// </summary>
        NextPlayer,

        /// <summary>
        /// Special rule (8-cut, spade-3 return, field reset) - same player continues
        /// </summary>
        SamePlayer,

        /// <summary>
        /// Skip finished players (for Phase 2+ with multiple finishers)
        /// </summary>
        SkipFinished,

        /// <summary>
        /// Game ended - stop turn progression
        /// </summary>
        GameEnd
    }

    /// <summary>
    /// Pure C# game logic for Daifugo (testable without Unity)
    /// Implements card play logic, special rules, and win conditions
    /// </summary>
    public class GameLogic
    {
        private readonly PlayableCardsCalculator calculator;
        private readonly GameRulesSO gameRules;

        /// <summary>
        /// Creates a new GameLogic instance
        /// </summary>
        /// <param name="gameRules">Game rules configuration</param>
        public GameLogic(GameRulesSO gameRules)
        {
            calculator = new PlayableCardsCalculator();
            this.gameRules = gameRules;
        }

        /// <summary>
        /// Executes card play logic with validation and special rules
        /// </summary>
        /// <param name="card">Card to play</param>
        /// <param name="hand">Player's hand</param>
        /// <param name="currentFieldState">Current field state (includes card history)</param>
        /// <returns>Result of the card play operation</returns>
        public CardPlayResult PlayCard(
            CardSO card,
            PlayerHandSO hand,
            FieldState currentFieldState)
        {
            // Validation 1: Check if card is in hand
            if (!calculator.IsCardInHand(card, hand))
            {
                return CardPlayResult.Fail("Card not in hand");
            }

            // Validation 2: Check if card can be played on current field (including binding check)
            if (!calculator.CanPlayCard(card, currentFieldState, gameRules))
            {
                return CardPlayResult.Fail("Cannot play this card on current field");
            }

            // Execute: Remove card from hand
            hand.RemoveCard(card);

            // Check special rules
            bool shouldResetField = CheckSpecialRules_8Cut(card);
            bool shouldActivate11Back = CheckSpecialRules_11Back(card);

            // Update field state with new card (apply 11-back if activated)
            FieldState newFieldState = FieldState.AddCard(currentFieldState, card, activates11Back: shouldActivate11Back);

            // Check win condition
            bool isWin = hand.IsEmpty;

            return CardPlayResult.Success(
                newFieldState: newFieldState,
                isWin: isWin,
                shouldResetField: shouldResetField,
                shouldActivate11Back: shouldActivate11Back
            );
        }

        /// <summary>
        /// Checks if 8-cut rule is activated
        /// </summary>
        /// <param name="card">Card that was played</param>
        /// <returns>True if field should reset immediately, false otherwise</returns>
        private bool CheckSpecialRules_8Cut(CardSO card)
        {
            // 8-cut rule: 8 clears the field immediately
            if (gameRules.Is8CutEnabled && card.Rank == 8)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if 11-back rule is activated
        /// </summary>
        /// <param name="card">Card that was played</param>
        /// <returns>True if 11-back should activate (temporary revolution), false otherwise</returns>
        private bool CheckSpecialRules_11Back(CardSO card)
        {
            // 11-back rule: J (rank 11) triggers temporary revolution
            if (gameRules.Is11BackEnabled && card.Rank == 11)
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Result of a card play operation
    /// </summary>
    public struct CardPlayResult
    {
        /// <summary>
        /// Whether the card play was successful
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// Whether the player won (hand is empty)
        /// </summary>
        public bool IsWin { get; private set; }

        /// <summary>
        /// Whether the field should reset immediately (e.g., 8-cut)
        /// </summary>
        public bool ShouldResetField { get; private set; }

        /// <summary>
        /// Whether 11-back rule is activated (J card triggers temporary revolution)
        /// </summary>
        public bool ShouldActivate11Back { get; private set; }

        /// <summary>
        /// The new field state after the play (includes card history for binding detection)
        /// </summary>
        public FieldState NewFieldState { get; private set; }

        /// <summary>
        /// Error message if play failed
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Turn advancement type based on the result
        /// Derived property: GameEnd if win, SamePlayer if field reset, otherwise NextPlayer
        /// </summary>
        public TurnAdvanceType TurnAdvanceType
        {
            get
            {
                if (IsWin) return TurnAdvanceType.GameEnd;
                if (ShouldResetField) return TurnAdvanceType.SamePlayer;
                return TurnAdvanceType.NextPlayer;
            }
        }

        /// <summary>
        /// Creates a successful result
        /// </summary>
        public static CardPlayResult Success(FieldState newFieldState, bool isWin, bool shouldResetField, bool shouldActivate11Back = false)
        {
            return new CardPlayResult
            {
                IsSuccess = true,
                IsWin = isWin,
                ShouldResetField = shouldResetField,
                ShouldActivate11Back = shouldActivate11Back,
                NewFieldState = newFieldState,
                ErrorMessage = null
            };
        }

        /// <summary>
        /// Creates a failed result
        /// </summary>
        public static CardPlayResult Fail(string errorMessage)
        {
            return new CardPlayResult
            {
                IsSuccess = false,
                IsWin = false,
                ShouldResetField = false,
                ShouldActivate11Back = false,
                NewFieldState = FieldState.Empty(),
                ErrorMessage = errorMessage
            };
        }
    }
}

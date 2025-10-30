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
        /// <summary>
        /// Executes card play logic with validation and special rules
        /// </summary>
        /// <param name="card">Card to play</param>
        /// <param name="hand">Player's hand</param>
        /// <param name="currentFieldCard">Current card on field (null if empty)</param>
        /// <param name="validator">Rule validator for game rules</param>
        /// <returns>Result of the card play operation</returns>
        public CardPlayResult PlayCard(
            CardSO card,
            PlayerHandSO hand,
            CardSO currentFieldCard,
            IRuleValidator validator)
        {
            // Validation 1: Check if card is in hand
            if (!validator.IsCardInHand(card, hand))
            {
                return CardPlayResult.Fail("Card not in hand");
            }

            // Validation 2: Check if card can be played on current field
            if (!validator.CanPlayCard(card, currentFieldCard))
            {
                return CardPlayResult.Fail("Cannot play this card on current field");
            }

            // Execute: Remove card from hand
            hand.RemoveCard(card);

            // Check special rules
            bool shouldResetField = CheckSpecialRules(card);

            // Check win condition
            bool isWin = hand.IsEmpty;

            return CardPlayResult.Success(
                newFieldCard: card,
                isWin: isWin,
                shouldResetField: shouldResetField
            );
        }

        /// <summary>
        /// Checks special rules for the played card
        /// </summary>
        /// <param name="card">Card that was played</param>
        /// <returns>True if field should reset immediately, false otherwise</returns>
        private bool CheckSpecialRules(CardSO card)
        {
            // 8-cut rule: 8 clears the field immediately
            if (card.Rank == 8)
            {
                return true;
            }

            // Future special rules (革命, 縛り, etc.) will be added here

            return false;
        }
    }

    /// <summary>
    /// Interface for rule validation (allows mocking in tests)
    /// </summary>
    public interface IRuleValidator
    {
        /// <summary>
        /// Checks if a card is in a player's hand
        /// </summary>
        bool IsCardInHand(CardSO card, PlayerHandSO hand);

        /// <summary>
        /// Checks if a card can be played on the current field
        /// </summary>
        bool CanPlayCard(CardSO card, CardSO currentFieldCard);
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
        /// The new field card after the play
        /// </summary>
        public CardSO NewFieldCard { get; private set; }

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
        public static CardPlayResult Success(CardSO newFieldCard, bool isWin, bool shouldResetField)
        {
            return new CardPlayResult
            {
                IsSuccess = true,
                IsWin = isWin,
                ShouldResetField = shouldResetField,
                NewFieldCard = newFieldCard,
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
                NewFieldCard = null,
                ErrorMessage = errorMessage
            };
        }
    }
}

using System.Linq;
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
        private readonly PlayPatternDetector patternDetector;
        private readonly GameRulesSO gameRules;

        /// <summary>
        /// Creates a new GameLogic instance
        /// </summary>
        /// <param name="gameRules">Game rules configuration</param>
        public GameLogic(GameRulesSO gameRules)
        {
            calculator = new PlayableCardsCalculator();
            patternDetector = new PlayPatternDetector();
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
            bool shouldResetField = CheckSpecialRules_8Cut(card) || CheckSpecialRules_Spade3Return(card, currentFieldState);
            bool shouldActivate11Back = CheckSpecialRules_11Back(card);

            // Update field state with new card (apply 11-back if activated)
            FieldState newFieldState = FieldState.AddCard(currentFieldState, card, activates11Back: shouldActivate11Back);

            // Check win condition
            bool isWin = hand.IsEmpty;

            // Check forbidden finish (禁止上がり)
            // If player finishes with forbidden card, they lose instead of winning
            bool isForbiddenFinish = isWin && CheckForbiddenFinish(card);

            return CardPlayResult.Success(
                newFieldState: newFieldState,
                isWin: isWin,
                shouldResetField: shouldResetField,
                shouldActivate11Back: shouldActivate11Back,
                isForbiddenFinish: isForbiddenFinish
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

        /// <summary>
        /// Checks if Spade 3 Return rule is activated
        /// </summary>
        /// <param name="card">Card that was played</param>
        /// <param name="currentFieldState">Current field state before this card was played</param>
        /// <returns>True if Spade 3 beats single Joker (field should reset), false otherwise</returns>
        private bool CheckSpecialRules_Spade3Return(CardSO card, FieldState currentFieldState)
        {
            // Spade 3 Return: Spade 3 beats single Joker and clears the field
            // Phase 1: 複数枚出しがないため、CurrentCardがJokerであればJoker単体プレイと判定
            if (gameRules.IsSpade3ReturnEnabled &&
                card.CardSuit == CardSO.Suit.Spade &&
                card.Rank == 3 &&
                currentFieldState.CurrentCard != null &&
                currentFieldState.CurrentCard.IsJoker)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the card is a forbidden finish card (禁止上がり)
        /// Forbidden cards: Joker, 2, 8, Spade 3
        /// </summary>
        /// <param name="card">Card that was played</param>
        /// <returns>True if card is forbidden for finishing, false otherwise</returns>
        private bool CheckForbiddenFinish(CardSO card)
        {
            if (!gameRules.IsForbiddenFinishEnabled) return false;

            // Joker is always forbidden
            if (card.IsJoker) return true;

            // 2, 8, Spade 3 are forbidden (Phase 1: no revolution, so always these cards)
            if (card.Rank == 2) return true;
            if (card.Rank == 8) return true;
            if (card.CardSuit == CardSO.Suit.Spade && card.Rank == 3) return true;

            return false;
        }

        // ========== Phase 1.5: Multiple Cards Play ==========

        /// <summary>
        /// Executes multiple cards play logic with validation and special rules
        /// Phase 1.5: Supports pairs, triples, quadruples, and sequences
        /// </summary>
        /// <param name="cards">Cards to play</param>
        /// <param name="hand">Player's hand (will be modified)</param>
        /// <param name="currentFieldState">Current field state</param>
        /// <returns>Result of the play</returns>
        public CardPlayResult PlayCards(System.Collections.Generic.List<CardSO> cards, PlayerHandSO hand, FieldState currentFieldState)
        {
            // Validate: Non-null, non-empty
            if (cards == null || cards.Count == 0)
                return CardPlayResult.Fail("Cannot play empty cards");

            // Validate: All cards in hand
            foreach (var card in cards)
            {
                if (!calculator.IsCardInHand(card, hand))
                    return CardPlayResult.Fail("Cannot play card not in hand");
            }

            // Validate: Pattern detection
            PlayPattern pattern = patternDetector.DetectPattern(cards);
            if (pattern == PlayPattern.Invalid)
                return CardPlayResult.Fail("Invalid card combination");

            // Validate: Can play on current field
            if (!CanPlayCards(cards, currentFieldState))
                return CardPlayResult.Fail("Cannot play these cards on current field");

            // Execute: Remove cards from hand
            foreach (var card in cards)
            {
                hand.RemoveCard(card);
            }

            // Check special rules
            bool shouldResetField = CheckSpecialRules_8Cut(cards);
            bool shouldActivate11Back = CheckSpecialRules_11Back(cards);
            bool shouldActivateRevolution = (pattern == PlayPattern.Quadruple) && gameRules.IsRevolutionEnabled;

            // Update field state with new cards
            FieldState newFieldState = FieldState.AddCards(
                currentFieldState,
                cards,
                playerID: 0,  // TODO: Pass actual player ID in Phase 2
                activatesRevolution: shouldActivateRevolution,
                activates11Back: shouldActivate11Back
            );

            // Check win condition
            bool isWin = hand.IsEmpty;

            // Check forbidden finish (禁止上がり)
            bool isForbiddenFinish = isWin && CheckForbiddenFinish(cards);

            return CardPlayResult.Success(
                newFieldState: newFieldState,
                isWin: isWin,
                shouldResetField: shouldResetField,
                shouldActivate11Back: shouldActivate11Back,
                shouldActivateRevolution: shouldActivateRevolution,
                isForbiddenFinish: isForbiddenFinish
            );
        }

        /// <summary>
        /// Checks if multiple cards can be played on current field
        /// </summary>
        private bool CanPlayCards(System.Collections.Generic.List<CardSO> cards, FieldState fieldState)
        {
            // Empty field: any valid pattern can be played
            if (fieldState.IsEmpty)
                return true;

            // Get pattern of cards to play
            PlayPattern playPattern = patternDetector.DetectPattern(cards);
            if (playPattern == PlayPattern.Invalid)
                return false;

            // Get last play pattern and count
            PlayPattern lastPattern = fieldState.GetLastPlayPattern();
            int lastCount = fieldState.GetLastPlayCount();

            // Card count must match (except for empty field)
            if (cards.Count != lastCount)
                return false;

            // Pattern must match (Single/Pair/Triple/Quadruple/Sequence)
            if (playPattern != lastPattern)
                return false;

            // Strength comparison based on pattern
            return CompareMultipleCardStrength(cards, fieldState, playPattern);
        }

        /// <summary>
        /// Compares strength of multiple cards play
        /// </summary>
        private bool CompareMultipleCardStrength(System.Collections.Generic.List<CardSO> cards, FieldState fieldState, PlayPattern pattern)
        {
            bool isRevolution = fieldState.GetEffectiveRevolution();

            if (pattern == PlayPattern.Sequence)
            {
                // Sequence: Compare by max (normal) or min (revolution) card
                int playStrength = patternDetector.GetSequenceStrength(cards, isRevolution);
                int fieldStrength = patternDetector.GetSequenceStrength(fieldState.CurrentPlay.Value.Cards.ToList(), isRevolution);
                return playStrength > fieldStrength;
            }
            else
            {
                // Same rank (Pair/Triple/Quadruple): Compare by single card strength
                // All cards have same rank, so just compare first card
                int playStrength = cards[0].GetStrength(isRevolution);
                int fieldStrength = fieldState.Strength;
                return playStrength > fieldStrength;
            }
        }

        /// <summary>
        /// Checks 8-cut rule for multiple cards
        /// </summary>
        private bool CheckSpecialRules_8Cut(System.Collections.Generic.List<CardSO> cards)
        {
            if (!gameRules.Is8CutEnabled) return false;

            // 8-cut: Any card with rank 8 triggers field reset
            return cards.Any(c => !c.IsJoker && c.Rank == 8);
        }

        /// <summary>
        /// Checks 11-back rule for multiple cards
        /// </summary>
        private bool CheckSpecialRules_11Back(System.Collections.Generic.List<CardSO> cards)
        {
            if (!gameRules.Is11BackEnabled) return false;

            // 11-back: Any card with rank 11 (J) triggers temporary revolution
            return cards.Any(c => !c.IsJoker && c.Rank == 11);
        }

        /// <summary>
        /// Checks forbidden finish for multiple cards
        /// </summary>
        private bool CheckForbiddenFinish(System.Collections.Generic.List<CardSO> cards)
        {
            if (!gameRules.IsForbiddenFinishEnabled) return false;

            // Forbidden finish: Any forbidden card in the combination
            foreach (var card in cards)
            {
                if (card.IsJoker) return true;
                if (card.Rank == 2) return true;
                if (card.Rank == 8) return true;
                if (card.CardSuit == CardSO.Suit.Spade && card.Rank == 3) return true;
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
        /// Whether revolution rule is activated (4 cards of same rank triggers permanent revolution)
        /// Phase 1.5: 4枚出しで革命発動
        /// </summary>
        public bool ShouldActivateRevolution { get; private set; }

        /// <summary>
        /// Whether this is a forbidden finish (player loses instead of wins)
        /// 禁止カード（ジョーカー、2、8、スペード3）で上がった場合、負けとなる
        /// </summary>
        public bool IsForbiddenFinish { get; private set; }

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
        /// Derived property: GameEnd if win or forbidden finish, SamePlayer if field reset, otherwise NextPlayer
        /// </summary>
        public TurnAdvanceType TurnAdvanceType
        {
            get
            {
                if (IsWin || IsForbiddenFinish) return TurnAdvanceType.GameEnd;
                if (ShouldResetField) return TurnAdvanceType.SamePlayer;
                return TurnAdvanceType.NextPlayer;
            }
        }

        /// <summary>
        /// Creates a successful result
        /// </summary>
        public static CardPlayResult Success(
            FieldState newFieldState,
            bool isWin,
            bool shouldResetField,
            bool shouldActivate11Back = false,
            bool shouldActivateRevolution = false,
            bool isForbiddenFinish = false)
        {
            return new CardPlayResult
            {
                IsSuccess = true,
                IsWin = isWin,
                ShouldResetField = shouldResetField,
                ShouldActivate11Back = shouldActivate11Back,
                ShouldActivateRevolution = shouldActivateRevolution,
                IsForbiddenFinish = isForbiddenFinish,
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
                ShouldActivateRevolution = false,
                IsForbiddenFinish = false,
                NewFieldState = FieldState.Empty(),
                ErrorMessage = errorMessage
            };
        }
    }
}

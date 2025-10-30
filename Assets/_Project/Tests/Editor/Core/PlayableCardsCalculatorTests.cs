using NUnit.Framework;
using Daifugo.Core;
using Daifugo.Data;
using Daifugo.Tests.Helpers;

namespace Daifugo.Tests.Core
{
    /// <summary>
    /// Tests for PlayableCardsCalculator class
    /// Validates playable card calculation logic according to Daifugo game rules
    /// Pure C# class - no Unity dependencies required for testing
    /// </summary>
    public class PlayableCardsCalculatorTests
    {
        private PlayableCardsCalculator calculator;
        private GameRulesSO defaultRules;

        /// <summary>
        /// Sets up test fixtures before each test
        /// Creates a pure C# PlayableCardsCalculator instance
        /// </summary>
        [SetUp]
        public void Setup()
        {
            calculator = new PlayableCardsCalculator();
            defaultRules = TestHelpers.CreateGameRules();
        }

        /// <summary>
        /// Cleans up after each test
        /// </summary>
        [TearDown]
        public void Teardown()
        {
            calculator = null;
            defaultRules = null;
        }

        #region GetPlayableCards Tests

        /// <summary>
        /// Test: All cards are playable when field is empty
        /// </summary>
        [Test]
        public void GetPlayableCards_OnEmptyField_ReturnsAllCards()
        {
            // Arrange
            var hand = TestHelpers.CreateHand(0,
                TestHelpers.CreateCardByRank(3),
                TestHelpers.CreateCardByRank(5),
                TestHelpers.CreateCardByRank(10));
            var fieldState = FieldState.Empty();

            // Act
            var result = calculator.GetPlayableCards(hand, fieldState, defaultRules);

            // Assert
            Assert.That(result.Count, Is.EqualTo(3), "All cards should be playable on empty field");
        }

        /// <summary>
        /// Test: Only stronger cards are playable
        /// </summary>
        [Test]
        public void GetPlayableCards_WithFieldCard_ReturnsStrongerCards()
        {
            // Arrange
            var hand = TestHelpers.CreateHand(0,
                TestHelpers.CreateCardByRank(3),  // Strength: 3
                TestHelpers.CreateCardByRank(5),  // Strength: 5
                TestHelpers.CreateCardByRank(10)); // Strength: 10
            var fieldCard = TestHelpers.CreateCardByRank(6); // Strength: 6
            var fieldState = FieldState.AddCard(FieldState.Empty(), fieldCard);

            // Act
            var result = calculator.GetPlayableCards(hand, fieldState, defaultRules);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1), "Only 10 should be playable (stronger than 6)");
            Assert.That(result[0].Rank, Is.EqualTo(10));
        }

        /// <summary>
        /// Test: No playable cards when all are weaker
        /// </summary>
        [Test]
        public void GetPlayableCards_AllWeaker_ReturnsEmpty()
        {
            // Arrange
            var hand = TestHelpers.CreateHand(0,
                TestHelpers.CreateCardByRank(3),
                TestHelpers.CreateCardByRank(5),
                TestHelpers.CreateCardByRank(7));
            var fieldCard = TestHelpers.CreateCardByRank(10); // Stronger than all
            var fieldState = FieldState.AddCard(FieldState.Empty(), fieldCard);

            // Act
            var result = calculator.GetPlayableCards(hand, fieldState, defaultRules);

            // Assert
            Assert.That(result.Count, Is.EqualTo(0), "No cards should be playable when all are weaker");
        }

        /// <summary>
        /// Test: Null hand returns empty list
        /// </summary>
        [Test]
        public void GetPlayableCards_NullHand_ReturnsEmpty()
        {
            // Arrange
            PlayerHandSO nullHand = null;
            var fieldState = FieldState.Empty();

            // Act
            var result = calculator.GetPlayableCards(nullHand, fieldState, defaultRules);

            // Assert
            Assert.That(result.Count, Is.EqualTo(0), "Null hand should return empty list");
        }

        #endregion

        #region CanPlayCard Tests

        /// <summary>
        /// Test: Any card can be played on empty field
        /// </summary>
        [Test]
        public void CanPlayCard_OnEmptyField_ReturnsTrue()
        {
            // Arrange
            var card = TestHelpers.CreateCardByRank(5);
            var fieldState = FieldState.Empty();

            // Act
            bool result = calculator.CanPlayCard(card, fieldState, defaultRules);

            // Assert
            Assert.IsTrue(result, "Any card should be playable on empty field");
        }

        /// <summary>
        /// Test: Stronger card can be played
        /// </summary>
        [Test]
        public void CanPlayCard_StrongerCard_ReturnsTrue()
        {
            // Arrange
            var weakCard = TestHelpers.CreateCardByRank(3);  // Strength: 3
            var strongCard = TestHelpers.CreateCardByRank(7); // Strength: 7
            var fieldState = FieldState.AddCard(FieldState.Empty(), weakCard);

            // Act
            bool result = calculator.CanPlayCard(strongCard, fieldState, defaultRules);

            // Assert
            Assert.IsTrue(result, "Stronger card should be playable");
        }

        /// <summary>
        /// Test: Weaker card cannot be played
        /// </summary>
        [Test]
        public void CanPlayCard_WeakerCard_ReturnsFalse()
        {
            // Arrange
            var weakCard = TestHelpers.CreateCardByRank(3);  // Strength: 3
            var strongCard = TestHelpers.CreateCardByRank(10); // Strength: 10
            var fieldState = FieldState.AddCard(FieldState.Empty(), strongCard);

            // Act
            bool result = calculator.CanPlayCard(weakCard, fieldState, defaultRules);

            // Assert
            Assert.IsFalse(result, "Weaker card should not be playable");
        }

        /// <summary>
        /// Test: Equal strength card cannot be played
        /// </summary>
        [Test]
        public void CanPlayCard_EqualStrength_ReturnsFalse()
        {
            // Arrange
            var card1 = TestHelpers.CreateCardByRank(7);
            var card2 = TestHelpers.CreateCardByRank(7);
            var fieldState = FieldState.AddCard(FieldState.Empty(), card1);

            // Act
            bool result = calculator.CanPlayCard(card2, fieldState, defaultRules);

            // Assert
            Assert.IsFalse(result, "Equal strength card should not be playable");
        }

        /// <summary>
        /// Test: Null card returns false
        /// </summary>
        [Test]
        public void CanPlayCard_NullCard_ReturnsFalse()
        {
            // Arrange
            CardSO nullCard = null;
            var fieldState = FieldState.Empty();

            // Act
            bool result = calculator.CanPlayCard(nullCard, fieldState, defaultRules);

            // Assert
            Assert.IsFalse(result, "Null card should not be playable");
        }

        #endregion

        #region IsCardInHand Tests

        /// <summary>
        /// Test: Card in hand returns true
        /// </summary>
        [Test]
        public void IsCardInHand_CardExists_ReturnsTrue()
        {
            // Arrange
            var card = TestHelpers.CreateCardByRank(5);
            var hand = TestHelpers.CreateHand(0, card);

            // Act
            bool result = calculator.IsCardInHand(card, hand);

            // Assert
            Assert.IsTrue(result, "Card in hand should return true");
        }

        /// <summary>
        /// Test: Card not in hand returns false
        /// </summary>
        [Test]
        public void IsCardInHand_CardDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var cardInHand = TestHelpers.CreateCardByRank(5);
            var cardNotInHand = TestHelpers.CreateCardByRank(10);
            var hand = TestHelpers.CreateHand(0, cardInHand);

            // Act
            bool result = calculator.IsCardInHand(cardNotInHand, hand);

            // Assert
            Assert.IsFalse(result, "Card not in hand should return false");
        }

        /// <summary>
        /// Test: Null card returns false
        /// </summary>
        [Test]
        public void IsCardInHand_NullCard_ReturnsFalse()
        {
            // Arrange
            CardSO nullCard = null;
            var hand = TestHelpers.CreateHand(0);

            // Act
            bool result = calculator.IsCardInHand(nullCard, hand);

            // Assert
            Assert.IsFalse(result, "Null card should return false");
        }

        /// <summary>
        /// Test: Null hand returns false
        /// </summary>
        [Test]
        public void IsCardInHand_NullHand_ReturnsFalse()
        {
            // Arrange
            var card = TestHelpers.CreateCardByRank(5);
            PlayerHandSO nullHand = null;

            // Act
            bool result = calculator.IsCardInHand(card, nullHand);

            // Assert
            Assert.IsFalse(result, "Null hand should return false");
        }

        #endregion

        #region Binding Rule Tests

        /// <summary>
        /// Test: Cannot play different suit during binding
        /// </summary>
        [Test]
        public void CanPlayCard_BindingActive_DifferentSuit_ReturnsFalse()
        {
            // Arrange
            CardSO spade5 = TestHelpers.CreateCard(CardSO.Suit.Spade, 5);
            CardSO spade7 = TestHelpers.CreateCard(CardSO.Suit.Spade, 7);
            CardSO heart10 = TestHelpers.CreateCard(CardSO.Suit.Heart, 10);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableBind: true);

            FieldState state = FieldState.AddCard(FieldState.Empty(), spade5);
            state = FieldState.AddCard(state, spade7);  // 縛り発動

            // Act
            bool canPlay = calculator.CanPlayCard(heart10, state, rules);

            // Assert
            Assert.IsFalse(canPlay, "縛り中は異なるスート出せない");
        }

        /// <summary>
        /// Test: Can play same suit stronger card during binding
        /// </summary>
        [Test]
        public void CanPlayCard_BindingActive_SameSuit_StrongerCard_ReturnsTrue()
        {
            // Arrange
            CardSO spade5 = TestHelpers.CreateCard(CardSO.Suit.Spade, 5);
            CardSO spade7 = TestHelpers.CreateCard(CardSO.Suit.Spade, 7);
            CardSO spade9 = TestHelpers.CreateCard(CardSO.Suit.Spade, 9);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableBind: true);

            FieldState state = FieldState.AddCard(FieldState.Empty(), spade5);
            state = FieldState.AddCard(state, spade7);  // 縛り発動

            // Act
            bool canPlay = calculator.CanPlayCard(spade9, state, rules);

            // Assert
            Assert.IsTrue(canPlay, "縛り中でも同じスートで強ければ出せる");
        }

        /// <summary>
        /// Test: Cannot play same suit weaker card during binding
        /// </summary>
        [Test]
        public void CanPlayCard_BindingActive_SameSuit_WeakerCard_ReturnsFalse()
        {
            // Arrange
            CardSO spade7 = TestHelpers.CreateCard(CardSO.Suit.Spade, 7);
            CardSO spade9 = TestHelpers.CreateCard(CardSO.Suit.Spade, 9);
            CardSO spade5 = TestHelpers.CreateCard(CardSO.Suit.Spade, 5);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableBind: true);

            FieldState state = FieldState.AddCard(FieldState.Empty(), spade7);
            state = FieldState.AddCard(state, spade9);  // 縛り発動

            // Act
            bool canPlay = calculator.CanPlayCard(spade5, state, rules);

            // Assert
            Assert.IsFalse(canPlay, "縛り中でも弱いカードは出せない");
        }

        /// <summary>
        /// Test: Binding does not affect when rule is disabled
        /// </summary>
        [Test]
        public void CanPlayCard_BindingDisabled_DifferentSuit_ReturnsTrue()
        {
            // Arrange
            CardSO spade5 = TestHelpers.CreateCard(CardSO.Suit.Spade, 5);
            CardSO spade7 = TestHelpers.CreateCard(CardSO.Suit.Spade, 7);
            CardSO heart10 = TestHelpers.CreateCard(CardSO.Suit.Heart, 10);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableBind: false);

            FieldState state = FieldState.AddCard(FieldState.Empty(), spade5);
            state = FieldState.AddCard(state, spade7);

            // Act
            bool canPlay = calculator.CanPlayCard(heart10, state, rules);

            // Assert
            Assert.IsTrue(canPlay, "縛りルール無効時は異なるスートも出せる");
        }

        /// <summary>
        /// Test: GetPlayableCards filters correctly during binding
        /// </summary>
        [Test]
        public void GetPlayableCards_BindingActive_ReturnsOnlySameSuit()
        {
            // Arrange
            CardSO spade5 = TestHelpers.CreateCard(CardSO.Suit.Spade, 5);
            CardSO spade7 = TestHelpers.CreateCard(CardSO.Suit.Spade, 7);
            CardSO spade9 = TestHelpers.CreateCard(CardSO.Suit.Spade, 9);
            CardSO heart10 = TestHelpers.CreateCard(CardSO.Suit.Heart, 10);
            CardSO diamond11 = TestHelpers.CreateCard(CardSO.Suit.Diamond, 11);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableBind: true);

            var hand = TestHelpers.CreateHand(0, spade9, heart10, diamond11);

            FieldState state = FieldState.AddCard(FieldState.Empty(), spade5);
            state = FieldState.AddCard(state, spade7);  // 縛り発動

            // Act
            var playableCards = calculator.GetPlayableCards(hand, state, rules);

            // Assert
            Assert.AreEqual(1, playableCards.Count, "縛り中はスペードのみ");
            Assert.AreEqual(spade9, playableCards[0], "スペード9のみ出せる");
        }

        #endregion
    }
}

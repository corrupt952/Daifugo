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
            Assert.IsFalse(canPlay, "Cannot play different suit during binding");
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
            Assert.IsTrue(canPlay, "Can play same suit stronger card during binding");
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
            Assert.IsFalse(canPlay, "Cannot play weaker card even with same suit during binding");
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
            Assert.IsTrue(canPlay, "Can play different suit when binding is disabled");
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
            Assert.AreEqual(1, playableCards.Count, "Only Spades are playable during binding");
            Assert.AreEqual(spade9, playableCards[0], "Only Spade 9 is playable");
        }

        #endregion

        #region 11-Back (Temporary Revolution) Tests

        /// <summary>
        /// Test: During 11-back (temporary revolution), weak cards can beat strong cards
        /// </summary>
        [Test]
        public void CanPlayCard_During11Back_WeakCardBeatsStrong()
        {
            // Arrange
            CardSO card5 = TestHelpers.CreateCardByRank(5); // Weak card
            CardSO card10 = TestHelpers.CreateCardByRank(10); // Strong card
            GameRulesSO rules = TestHelpers.CreateGameRules(enable11Back: true);

            // Create field with J (activates 11-back)
            CardSO cardJ = TestHelpers.CreateCardByRank(11);
            FieldState state = FieldState.AddCard(FieldState.Empty(), cardJ, activates11Back: true);
            // Then add card10
            state = FieldState.AddCard(state, card10);

            // Act - Try to play weaker card (5)
            bool canPlay = calculator.CanPlayCard(card5, state, rules);

            // Assert
            Assert.IsTrue(canPlay, "During 11-back, 5 should beat 10 (strength reversed)");
            Assert.IsTrue(state.IsTemporaryRevolution, "Field should have temporary revolution");
        }

        /// <summary>
        /// Test: During 11-back, strong cards cannot beat weak cards
        /// </summary>
        [Test]
        public void CanPlayCard_During11Back_StrongCardCannotBeatWeak()
        {
            // Arrange
            CardSO card3 = TestHelpers.CreateCardByRank(3); // Very weak card
            CardSO cardK = TestHelpers.CreateCardByRank(13); // King (strong normally)
            GameRulesSO rules = TestHelpers.CreateGameRules(enable11Back: true);

            // Create field with J (activates 11-back)
            CardSO cardJ = TestHelpers.CreateCardByRank(11);
            FieldState state = FieldState.AddCard(FieldState.Empty(), cardJ, activates11Back: true);
            // Then add weak card (3)
            state = FieldState.AddCard(state, card3);

            // Act - Try to play strong card (K)
            bool canPlay = calculator.CanPlayCard(cardK, state, rules);

            // Assert
            Assert.IsFalse(canPlay, "During 11-back, K cannot beat 3 (strength reversed)");
        }

        /// <summary>
        /// Test: Without 11-back, normal strength comparison applies
        /// </summary>
        [Test]
        public void CanPlayCard_No11Back_NormalStrengthComparison()
        {
            // Arrange
            CardSO card5 = TestHelpers.CreateCardByRank(5);
            CardSO card10 = TestHelpers.CreateCardByRank(10);
            GameRulesSO rules = TestHelpers.CreateGameRules(enable11Back: false);

            FieldState state = FieldState.AddCard(FieldState.Empty(), card5);

            // Act - Try to play stronger card
            bool canPlay = calculator.CanPlayCard(card10, state, rules);

            // Assert
            Assert.IsTrue(canPlay, "Without 11-back, normal strength: 10 beats 5");
            Assert.IsFalse(state.IsTemporaryRevolution, "No temporary revolution");
        }

        /// <summary>
        /// Test: After field reset, 11-back revolution is cleared
        /// </summary>
        [Test]
        public void CanPlayCard_AfterFieldReset_RevolutionCleared()
        {
            // Arrange
            CardSO card5 = TestHelpers.CreateCardByRank(5);
            CardSO card10 = TestHelpers.CreateCardByRank(10);
            CardSO cardJ = TestHelpers.CreateCardByRank(11);
            GameRulesSO rules = TestHelpers.CreateGameRules(enable11Back: true);

            // Create field with J (activates 11-back)
            FieldState state = FieldState.AddCard(FieldState.Empty(), cardJ, activates11Back: true);
            Assert.IsTrue(state.IsTemporaryRevolution, "Revolution should be active");

            // Field reset (simulating field clear after all players pass)
            FieldState clearedState = FieldState.Empty();

            // Add card5 to cleared field
            clearedState = FieldState.AddCard(clearedState, card5);

            // Act - Try to play card10 (should work with normal strength)
            bool canPlay = calculator.CanPlayCard(card10, clearedState, rules);

            // Assert
            Assert.IsTrue(canPlay, "After field reset, normal strength: 10 beats 5");
            Assert.IsFalse(clearedState.IsTemporaryRevolution, "Field reset clears revolution");
        }

        /// <summary>
        /// Test: GetPlayableCards returns correct cards during 11-back
        /// </summary>
        [Test]
        public void GetPlayableCards_During11Back_ReturnsWeakerCards()
        {
            // Arrange
            CardSO card3 = TestHelpers.CreateCardByRank(3);
            CardSO card5 = TestHelpers.CreateCardByRank(5);
            CardSO card10 = TestHelpers.CreateCardByRank(10);
            CardSO cardJ = TestHelpers.CreateCardByRank(11);
            GameRulesSO rules = TestHelpers.CreateGameRules(enable11Back: true);

            var hand = TestHelpers.CreateHand(0, card3, card5, card10);

            // Create field with J (activates 11-back) and card10 on top
            FieldState state = FieldState.AddCard(FieldState.Empty(), cardJ, activates11Back: true);
            state = FieldState.AddCard(state, card10); // Current card: 10 (strength 10)

            // Act
            var playableCards = calculator.GetPlayableCards(hand, state, rules);

            // Assert
            Assert.AreEqual(2, playableCards.Count, "During 11-back, 3 and 5 should be playable (weaker than 10)");
            Assert.Contains(card3, playableCards, "Card 3 should be playable");
            Assert.Contains(card5, playableCards, "Card 5 should be playable");
            Assert.IsFalse(playableCards.Contains(card10), "Card 10 cannot play on itself");
        }

        /// <summary>
        /// Test: 11-back disabled, normal playable cards calculation
        /// </summary>
        [Test]
        public void GetPlayableCards_11BackDisabled_NormalCalculation()
        {
            // Arrange
            CardSO card3 = TestHelpers.CreateCardByRank(3);
            CardSO card5 = TestHelpers.CreateCardByRank(5);
            CardSO card10 = TestHelpers.CreateCardByRank(10);
            CardSO cardJ = TestHelpers.CreateCardByRank(11);
            GameRulesSO rules = TestHelpers.CreateGameRules(enable11Back: false);

            var hand = TestHelpers.CreateHand(0, card3, card10);

            // Create field with J (but 11-back is disabled)
            FieldState state = FieldState.AddCard(FieldState.Empty(), cardJ);
            state = FieldState.AddCard(state, card5); // Current card: 5 (strength 5)

            // Act
            var playableCards = calculator.GetPlayableCards(hand, state, rules);

            // Assert
            Assert.AreEqual(1, playableCards.Count, "Only stronger cards should be playable");
            Assert.Contains(card10, playableCards, "Card 10 beats 5 (normal)");
            Assert.IsFalse(playableCards.Contains(card3), "Card 3 cannot beat 5 (normal)");
        }

        /// <summary>
        /// Test: During 11-back on empty field, all cards are still playable
        /// </summary>
        [Test]
        public void CanPlayCard_11BackWithEmptyField_AllPlayable()
        {
            // Arrange
            CardSO card3 = TestHelpers.CreateCardByRank(3);
            GameRulesSO rules = TestHelpers.CreateGameRules(enable11Back: true);

            // Create field with J, then reset (simulating field reset after 11-back)
            CardSO cardJ = TestHelpers.CreateCardByRank(11);
            FieldState state = FieldState.AddCard(FieldState.Empty(), cardJ, activates11Back: true);
            // Field is reset (but revolution state should persist until field actually clears)
            // In this test, we're testing Empty() which should clear revolution
            FieldState emptyState = FieldState.Empty();

            // Act
            bool canPlay = calculator.CanPlayCard(card3, emptyState, rules);

            // Assert
            Assert.IsTrue(canPlay, "On empty field, all cards are playable");
            Assert.IsFalse(emptyState.IsTemporaryRevolution, "Empty field clears revolution");
        }

        /// <summary>
        /// Test: Ace (1) and Two (2) strength is also reversed during 11-back
        /// </summary>
        [Test]
        public void CanPlayCard_During11Back_AceAndTwoReversed()
        {
            // Arrange
            CardSO card3 = TestHelpers.CreateCardByRank(3); // Strength: 3
            CardSO ace = TestHelpers.CreateCardByRank(1);   // Strength: 14 (normally strongest)
            CardSO two = TestHelpers.CreateCardByRank(2);   // Strength: 15 (normally strongest)
            CardSO cardJ = TestHelpers.CreateCardByRank(11);
            GameRulesSO rules = TestHelpers.CreateGameRules(enable11Back: true);

            // Create field with J (activates 11-back) and Two on top
            FieldState state = FieldState.AddCard(FieldState.Empty(), cardJ, activates11Back: true);
            state = FieldState.AddCard(state, two); // Current: Two (strength 15, highest)

            // Act - Try to play weak card (3)
            bool canPlay3 = calculator.CanPlayCard(card3, state, rules);
            bool canPlayAce = calculator.CanPlayCard(ace, state, rules);

            // Assert
            Assert.IsTrue(canPlay3, "During 11-back, 3 beats 2 (reversed)");
            Assert.IsTrue(canPlayAce, "During 11-back, Ace beats 2 (reversed, but still higher than 3)");
        }

        /// <summary>
        /// Test: During 11-back, J follows reversed strength rules (hard to play)
        /// </summary>
        [Test]
        public void CanPlayCard_During11Back_JFollowsReversedRules()
        {
            // Arrange
            CardSO cardJ1 = TestHelpers.CreateCardByRank(11); // Strength: 11
            CardSO cardJ2 = TestHelpers.CreateCardByRank(11); // Strength: 11
            CardSO card3 = TestHelpers.CreateCardByRank(3);   // Strength: 3
            GameRulesSO rules = TestHelpers.CreateGameRules(enable11Back: true);

            // Create field with first J (activates 11-back)
            FieldState state = FieldState.AddCard(FieldState.Empty(), cardJ1, activates11Back: true);

            // Act - Try to play second J during 11-back (11 < 11? No!)
            bool canPlayJ = calculator.CanPlayCard(cardJ2, state, rules);

            // Try to play weak card (3 < 11? Yes!)
            bool canPlay3 = calculator.CanPlayCard(card3, state, rules);

            // Assert
            Assert.IsFalse(canPlayJ, "J cannot play on J during 11-back (11 < 11 is false)");
            Assert.IsTrue(canPlay3, "Weak card (3) can play on J during 11-back (3 < 11)");
            Assert.IsTrue(state.IsTemporaryRevolution, "Revolution should be active");
        }

        #endregion

        #region Spade 3 Return Tests

        /// <summary>
        /// Test: Spade 3 can beat single Joker when rule is enabled
        /// </summary>
        [Test]
        public void CanPlayCard_Spade3OnSingleJoker_ReturnsTrue()
        {
            // Arrange
            CardSO joker = TestHelpers.CreateJoker(isRed: true);
            CardSO spade3 = TestHelpers.CreateCard(CardSO.Suit.Spade, 3);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableSpade3Return: true);

            // Create field with single Joker
            FieldState state = FieldState.AddCard(FieldState.Empty(), joker);

            // Act
            bool canPlay = calculator.CanPlayCard(spade3, state, rules);

            // Assert
            Assert.IsTrue(canPlay, "Spade 3 should beat single Joker when rule is enabled");
        }

        /// <summary>
        /// Test: Spade 3 cannot beat Joker when rule is disabled
        /// </summary>
        [Test]
        public void CanPlayCard_Spade3OnJoker_RuleDisabled_ReturnsFalse()
        {
            // Arrange
            CardSO joker = TestHelpers.CreateJoker(isRed: true);
            CardSO spade3 = TestHelpers.CreateCard(CardSO.Suit.Spade, 3);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableSpade3Return: false);

            // Create field with single Joker
            FieldState state = FieldState.AddCard(FieldState.Empty(), joker);

            // Act
            bool canPlay = calculator.CanPlayCard(spade3, state, rules);

            // Assert
            Assert.IsFalse(canPlay, "Spade 3 cannot beat Joker when rule is disabled (3 < 16)");
        }

        /// <summary>
        /// Test: Non-Spade 3 cards cannot beat Joker
        /// </summary>
        [Test]
        public void CanPlayCard_NonSpade3OnJoker_ReturnsFalse()
        {
            // Arrange
            CardSO joker = TestHelpers.CreateJoker(isRed: true);
            CardSO heart3 = TestHelpers.CreateCard(CardSO.Suit.Heart, 3);
            CardSO spade4 = TestHelpers.CreateCard(CardSO.Suit.Spade, 4);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableSpade3Return: true);

            // Create field with single Joker
            FieldState state = FieldState.AddCard(FieldState.Empty(), joker);

            // Act
            bool canPlayHeart3 = calculator.CanPlayCard(heart3, state, rules);
            bool canPlaySpade4 = calculator.CanPlayCard(spade4, state, rules);

            // Assert
            Assert.IsFalse(canPlayHeart3, "Heart 3 cannot beat Joker (only Spade 3)");
            Assert.IsFalse(canPlaySpade4, "Spade 4 cannot beat Joker (only Spade 3)");
        }

        /// <summary>
        /// Test: Spade 3 ignores binding when beating Joker
        /// </summary>
        [Test]
        public void CanPlayCard_Spade3OnJoker_IgnoresBinding()
        {
            // Arrange
            CardSO heartCard = TestHelpers.CreateCard(CardSO.Suit.Heart, 5);
            CardSO heartCard2 = TestHelpers.CreateCard(CardSO.Suit.Heart, 7);
            CardSO joker = TestHelpers.CreateJoker(isRed: true);
            CardSO spade3 = TestHelpers.CreateCard(CardSO.Suit.Spade, 3);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableBind: true, enableSpade3Return: true);

            // Create field with Hearts binding (2 Hearts in a row), then Joker
            FieldState state = FieldState.AddCard(FieldState.Empty(), heartCard);
            state = FieldState.AddCard(state, heartCard2);
            state = FieldState.AddCard(state, joker); // Joker on top, but binding still active

            // Act
            bool canPlay = calculator.CanPlayCard(spade3, state, rules);

            // Assert
            Assert.IsTrue(canPlay, "Spade 3 should ignore binding when beating Joker");
        }

        /// <summary>
        /// Test: Spade 3 can beat Joker even when multiple cards were played sequentially
        /// Phase 1: 1枚ずつプレイするため、CurrentCardがJokerならスペ3返し適用
        /// Phase 2実装時: 複数枚同時出しの場合は別途判定が必要
        /// </summary>
        [Test]
        public void CanPlayCard_Spade3OnJokerAfterMultipleCards_ReturnsTrue()
        {
            // Arrange
            CardSO joker1 = TestHelpers.CreateJoker(isRed: true);
            CardSO joker2 = TestHelpers.CreateJoker(isRed: false);
            CardSO spade3 = TestHelpers.CreateCard(CardSO.Suit.Spade, 3);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableSpade3Return: true);

            // Create field with sequential plays: Joker1, then Joker2
            // Phase 1: Each play is single card, so CurrentCard=Joker2 triggers Spade 3 Return
            FieldState state = FieldState.AddCard(FieldState.Empty(), joker1);
            state = FieldState.AddCard(state, joker2);

            // Act
            bool canPlay = calculator.CanPlayCard(spade3, state, rules);

            // Assert
            Assert.IsTrue(canPlay, "Phase 1: Spade 3 can beat Joker (CurrentCard=Joker2)");
        }

        /// <summary>
        /// Test: Spade 3 Return only applies when last card is Joker
        /// </summary>
        [Test]
        public void CanPlayCard_Spade3_OnlyAppliesWhenTopCardIsJoker()
        {
            // Arrange
            CardSO joker = TestHelpers.CreateJoker(isRed: true);
            CardSO card5 = TestHelpers.CreateCardByRank(5);
            CardSO spade3 = TestHelpers.CreateCard(CardSO.Suit.Spade, 3);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableSpade3Return: true);

            // Create field with Joker, then 5 on top (so top is not Joker)
            // NOTE: In real game, you can't play 5 on Joker, but this is for testing logic
            FieldState state = FieldState.AddCard(FieldState.Empty(), card5);

            // Act
            bool canPlay = calculator.CanPlayCard(spade3, state, rules);

            // Assert
            Assert.IsFalse(canPlay, "Spade 3 Return only applies when field has single Joker (3 < 5)");
        }

        #endregion
    }
}

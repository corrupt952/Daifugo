using NUnit.Framework;
using Daifugo.Core;
using Daifugo.Data;
using Daifugo.Tests.Helpers;

namespace Daifugo.Tests.Core
{
    /// <summary>
    /// Tests for FieldState struct
    /// Validates card history management and binding rule detection
    /// </summary>
    public class FieldStateTests
    {
        #region Empty Field Tests

        /// <summary>
        /// Test: Empty field has no cards
        /// </summary>
        [Test]
        public void Empty_ReturnsEmptyState()
        {
            // Act
            FieldState state = FieldState.Empty();

            // Assert
            Assert.IsTrue(state.IsEmpty, "Empty field should be empty");
            Assert.AreEqual(0, state.CardsInField.Count, "Empty field should have 0 cards");
            Assert.IsNull(state.CurrentCard, "Empty field should have null current card");
            Assert.AreEqual(0, state.Strength, "Empty field should have 0 strength");
        }

        #endregion

        #region AddCard Tests

        /// <summary>
        /// Test: Adding card to empty field creates new state with one card
        /// </summary>
        [Test]
        public void AddCard_EmptyField_AddsCard()
        {
            // Arrange
            CardSO card = TestHelpers.CreateCard(CardSO.Suit.Spade, 5);
            FieldState empty = FieldState.Empty();

            // Act
            FieldState state = FieldState.AddCard(empty, card);

            // Assert
            Assert.IsFalse(state.IsEmpty, "Field should not be empty");
            Assert.AreEqual(1, state.CardsInField.Count, "Field should have 1 card");
            Assert.AreEqual(card, state.CurrentCard, "Current card should be the added card");
            Assert.AreEqual(5, state.Strength, "Strength should be 5");
        }

        /// <summary>
        /// Test: Adding multiple cards creates history
        /// </summary>
        [Test]
        public void AddCard_MultipleCards_CreatesHistory()
        {
            // Arrange
            CardSO card1 = TestHelpers.CreateCard(CardSO.Suit.Spade, 5);
            CardSO card2 = TestHelpers.CreateCard(CardSO.Suit.Heart, 7);
            CardSO card3 = TestHelpers.CreateCard(CardSO.Suit.Diamond, 9);

            // Act
            FieldState state1 = FieldState.AddCard(FieldState.Empty(), card1);
            FieldState state2 = FieldState.AddCard(state1, card2);
            FieldState state3 = FieldState.AddCard(state2, card3);

            // Assert
            Assert.AreEqual(3, state3.CardsInField.Count, "Field should have 3 cards");
            Assert.AreEqual(card1, state3.CardsInField[0], "First card should be card1");
            Assert.AreEqual(card2, state3.CardsInField[1], "Second card should be card2");
            Assert.AreEqual(card3, state3.CardsInField[2], "Third card should be card3");
            Assert.AreEqual(card3, state3.CurrentCard, "Current card should be card3");
            Assert.AreEqual(9, state3.Strength, "Strength should be 9");
        }

        /// <summary>
        /// Test: AddCard is immutable (original state unchanged)
        /// </summary>
        [Test]
        public void AddCard_IsImmutable()
        {
            // Arrange
            CardSO card1 = TestHelpers.CreateCard(CardSO.Suit.Spade, 5);
            CardSO card2 = TestHelpers.CreateCard(CardSO.Suit.Heart, 7);
            FieldState state1 = FieldState.AddCard(FieldState.Empty(), card1);

            // Act
            FieldState state2 = FieldState.AddCard(state1, card2);

            // Assert
            Assert.AreEqual(1, state1.CardsInField.Count, "Original state should be unchanged");
            Assert.AreEqual(2, state2.CardsInField.Count, "New state should have 2 cards");
        }

        #endregion

        #region Binding Rule Tests

        /// <summary>
        /// Test: Binding activates when two consecutive cards have same suit
        /// </summary>
        [Test]
        public void IsBindingActive_TwoConsecutiveSameSuit_ReturnsTrue()
        {
            // Arrange
            CardSO card1 = TestHelpers.CreateCard(CardSO.Suit.Spade, 5);
            CardSO card2 = TestHelpers.CreateCard(CardSO.Suit.Spade, 7);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableBind: true);

            FieldState state1 = FieldState.AddCard(FieldState.Empty(), card1);
            FieldState state2 = FieldState.AddCard(state1, card2);

            // Act & Assert
            Assert.IsFalse(state1.IsBindingActive(rules), "1枚目では縛りなし");
            Assert.IsTrue(state2.IsBindingActive(rules), "2枚目で縛り発動");
        }

        /// <summary>
        /// Test: Binding does not activate with different suits
        /// </summary>
        [Test]
        public void IsBindingActive_TwoDifferentSuit_ReturnsFalse()
        {
            // Arrange
            CardSO card1 = TestHelpers.CreateCard(CardSO.Suit.Spade, 5);
            CardSO card2 = TestHelpers.CreateCard(CardSO.Suit.Heart, 7);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableBind: true);

            FieldState state1 = FieldState.AddCard(FieldState.Empty(), card1);
            FieldState state2 = FieldState.AddCard(state1, card2);

            // Act & Assert
            Assert.IsFalse(state2.IsBindingActive(rules), "異なるスートでは縛りなし");
        }

        /// <summary>
        /// Test: Binding does not activate when rule is disabled
        /// </summary>
        [Test]
        public void IsBindingActive_BindingDisabled_ReturnsFalse()
        {
            // Arrange
            CardSO card1 = TestHelpers.CreateCard(CardSO.Suit.Spade, 5);
            CardSO card2 = TestHelpers.CreateCard(CardSO.Suit.Spade, 7);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableBind: false);

            FieldState state1 = FieldState.AddCard(FieldState.Empty(), card1);
            FieldState state2 = FieldState.AddCard(state1, card2);

            // Act & Assert
            Assert.IsFalse(state2.IsBindingActive(rules), "ルール無効時は縛りなし");
        }

        /// <summary>
        /// Test: Binding continues with three consecutive same suit
        /// </summary>
        [Test]
        public void IsBindingActive_ThreeConsecutiveSameSuit_ReturnsTrue()
        {
            // Arrange
            CardSO card1 = TestHelpers.CreateCard(CardSO.Suit.Spade, 5);
            CardSO card2 = TestHelpers.CreateCard(CardSO.Suit.Spade, 7);
            CardSO card3 = TestHelpers.CreateCard(CardSO.Suit.Spade, 9);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableBind: true);

            FieldState state1 = FieldState.AddCard(FieldState.Empty(), card1);
            FieldState state2 = FieldState.AddCard(state1, card2);
            FieldState state3 = FieldState.AddCard(state2, card3);

            // Act & Assert
            Assert.IsTrue(state3.IsBindingActive(rules), "縛り継続");
        }

        /// <summary>
        /// Test: Binding breaks when different suit is played after binding
        /// </summary>
        [Test]
        public void IsBindingActive_DifferentSuitAfterBinding_ReturnsFalse()
        {
            // Arrange
            CardSO card1 = TestHelpers.CreateCard(CardSO.Suit.Spade, 5);
            CardSO card2 = TestHelpers.CreateCard(CardSO.Suit.Spade, 7);
            CardSO card3 = TestHelpers.CreateCard(CardSO.Suit.Heart, 9);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableBind: true);

            FieldState state1 = FieldState.AddCard(FieldState.Empty(), card1);
            FieldState state2 = FieldState.AddCard(state1, card2);
            FieldState state3 = FieldState.AddCard(state2, card3);

            // Act & Assert
            Assert.IsTrue(state2.IsBindingActive(rules), "2枚目で縛り発動");
            Assert.IsFalse(state3.IsBindingActive(rules), "異なるスートで縛り解除");
        }

        #endregion

        #region GetBindingSuit Tests

        /// <summary>
        /// Test: GetBindingSuit returns suit when binding is active
        /// </summary>
        [Test]
        public void GetBindingSuit_BindingActive_ReturnsSuit()
        {
            // Arrange
            CardSO card1 = TestHelpers.CreateCard(CardSO.Suit.Spade, 5);
            CardSO card2 = TestHelpers.CreateCard(CardSO.Suit.Spade, 7);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableBind: true);

            FieldState state1 = FieldState.AddCard(FieldState.Empty(), card1);
            FieldState state2 = FieldState.AddCard(state1, card2);

            // Act
            CardSO.Suit? bindingSuit = state2.GetBindingSuit(rules);

            // Assert
            Assert.IsNotNull(bindingSuit, "Binding suit should not be null");
            Assert.AreEqual(CardSO.Suit.Spade, bindingSuit.Value, "Binding suit should be Spade");
        }

        /// <summary>
        /// Test: GetBindingSuit returns null when binding is not active
        /// </summary>
        [Test]
        public void GetBindingSuit_BindingNotActive_ReturnsNull()
        {
            // Arrange
            CardSO card1 = TestHelpers.CreateCard(CardSO.Suit.Spade, 5);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableBind: true);

            FieldState state = FieldState.AddCard(FieldState.Empty(), card1);

            // Act
            CardSO.Suit? bindingSuit = state.GetBindingSuit(rules);

            // Assert
            Assert.IsNull(bindingSuit, "Binding suit should be null when not active");
        }

        /// <summary>
        /// Test: GetBindingSuit returns null when rule is disabled
        /// </summary>
        [Test]
        public void GetBindingSuit_RuleDisabled_ReturnsNull()
        {
            // Arrange
            CardSO card1 = TestHelpers.CreateCard(CardSO.Suit.Spade, 5);
            CardSO card2 = TestHelpers.CreateCard(CardSO.Suit.Spade, 7);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableBind: false);

            FieldState state1 = FieldState.AddCard(FieldState.Empty(), card1);
            FieldState state2 = FieldState.AddCard(state1, card2);

            // Act
            CardSO.Suit? bindingSuit = state2.GetBindingSuit(rules);

            // Assert
            Assert.IsNull(bindingSuit, "Binding suit should be null when rule is disabled");
        }

        #endregion

        #region 11-Back (Temporary Revolution) Tests

        /// <summary>
        /// Test: Empty field has no temporary revolution
        /// </summary>
        [Test]
        public void Empty_NoTemporaryRevolution()
        {
            // Act
            FieldState state = FieldState.Empty();

            // Assert
            Assert.IsFalse(state.IsTemporaryRevolution, "Empty field should not have temporary revolution");
        }

        /// <summary>
        /// Test: Adding card with 11-back activates temporary revolution
        /// </summary>
        [Test]
        public void AddCard_With11Back_ActivatesTemporaryRevolution()
        {
            // Arrange
            CardSO card = TestHelpers.CreateCardByRank(11); // J
            FieldState empty = FieldState.Empty();

            // Act
            FieldState state = FieldState.AddCard(empty, card, activates11Back: true);

            // Assert
            Assert.IsTrue(state.IsTemporaryRevolution, "11-back should activate temporary revolution");
            Assert.IsFalse(empty.IsTemporaryRevolution, "Original state should remain unchanged");
        }

        /// <summary>
        /// Test: Adding card without 11-back maintains revolution state
        /// </summary>
        [Test]
        public void AddCard_Without11Back_MaintainsRevolutionState()
        {
            // Arrange
            CardSO cardJ = TestHelpers.CreateCardByRank(11);
            CardSO cardNormal = TestHelpers.CreateCardByRank(5);
            FieldState empty = FieldState.Empty();

            // Act - First activate 11-back
            FieldState state1 = FieldState.AddCard(empty, cardJ, activates11Back: true);
            // Then add normal card
            FieldState state2 = FieldState.AddCard(state1, cardNormal, activates11Back: false);

            // Assert
            Assert.IsTrue(state1.IsTemporaryRevolution, "State1 should have revolution");
            Assert.IsTrue(state2.IsTemporaryRevolution, "State2 should maintain revolution");
        }

        /// <summary>
        /// Test: Second 11-back deactivates temporary revolution
        /// </summary>
        [Test]
        public void AddCard_Second11Back_DeactivatesTemporaryRevolution()
        {
            // Arrange
            CardSO cardJ1 = TestHelpers.CreateCardByRank(11);
            CardSO cardJ2 = TestHelpers.CreateCardByRank(11);
            FieldState empty = FieldState.Empty();

            // Act - First 11-back
            FieldState state1 = FieldState.AddCard(empty, cardJ1, activates11Back: true);
            // Second 11-back
            FieldState state2 = FieldState.AddCard(state1, cardJ2, activates11Back: true);

            // Assert
            Assert.IsFalse(empty.IsTemporaryRevolution, "Empty should have no revolution");
            Assert.IsTrue(state1.IsTemporaryRevolution, "First 11-back should activate");
            Assert.IsFalse(state2.IsTemporaryRevolution, "Second 11-back should deactivate");
        }

        /// <summary>
        /// Test: Empty clears temporary revolution
        /// </summary>
        [Test]
        public void Empty_ClearsTemporaryRevolution()
        {
            // Arrange
            CardSO cardJ = TestHelpers.CreateCardByRank(11);
            FieldState state = FieldState.AddCard(FieldState.Empty(), cardJ, activates11Back: true);

            // Act
            FieldState cleared = FieldState.Empty();

            // Assert
            Assert.IsTrue(state.IsTemporaryRevolution, "State should have revolution");
            Assert.IsFalse(cleared.IsTemporaryRevolution, "Cleared state should not have revolution");
        }

        /// <summary>
        /// Test: Third 11-back reactivates temporary revolution
        /// </summary>
        [Test]
        public void AddCard_Third11Back_ReactivatesTemporaryRevolution()
        {
            // Arrange
            CardSO cardJ1 = TestHelpers.CreateCardByRank(11);
            CardSO cardJ2 = TestHelpers.CreateCardByRank(11);
            CardSO cardJ3 = TestHelpers.CreateCardByRank(11);

            // Act
            FieldState state1 = FieldState.AddCard(FieldState.Empty(), cardJ1, activates11Back: true);
            FieldState state2 = FieldState.AddCard(state1, cardJ2, activates11Back: true);
            FieldState state3 = FieldState.AddCard(state2, cardJ3, activates11Back: true);

            // Assert
            Assert.IsTrue(state1.IsTemporaryRevolution, "First 11-back: revolution ON");
            Assert.IsFalse(state2.IsTemporaryRevolution, "Second 11-back: revolution OFF");
            Assert.IsTrue(state3.IsTemporaryRevolution, "Third 11-back: revolution ON again");
        }

        #endregion

        #region Edge Cases

        /// <summary>
        /// Test: Empty field clears history
        /// </summary>
        [Test]
        public void Empty_ClearsHistory()
        {
            // Arrange
            CardSO card1 = TestHelpers.CreateCard(CardSO.Suit.Spade, 5);
            FieldState state = FieldState.AddCard(FieldState.Empty(), card1);

            // Act
            FieldState cleared = FieldState.Empty();

            // Assert
            Assert.IsTrue(cleared.IsEmpty, "Cleared field should be empty");
            Assert.AreEqual(0, cleared.CardsInField.Count, "Cleared field should have 0 cards");
        }

        /// <summary>
        /// Test: Strength correctly reflects current card
        /// </summary>
        [Test]
        public void Strength_ReflectsCurrentCard()
        {
            // Arrange
            CardSO ace = TestHelpers.CreateCard(CardSO.Suit.Spade, 1);  // Strength: 14
            CardSO two = TestHelpers.CreateCard(CardSO.Suit.Heart, 2);  // Strength: 15

            // Act
            FieldState state1 = FieldState.AddCard(FieldState.Empty(), ace);
            FieldState state2 = FieldState.AddCard(state1, two);

            // Assert
            Assert.AreEqual(14, state1.Strength, "Ace strength should be 14");
            Assert.AreEqual(15, state2.Strength, "Two strength should be 15");
        }

        /// <summary>
        /// Test: Joker does not trigger binding (Joker has no suit)
        /// </summary>
        [Test]
        public void IsBindingActive_JokerAsSecondCard_ReturnsFalse()
        {
            // Arrange
            CardSO spadeCard = TestHelpers.CreateCard(CardSO.Suit.Spade, 5);
            CardSO joker = TestHelpers.CreateJoker(isRed: true);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableBind: true);

            // Act
            FieldState state = FieldState.AddCard(FieldState.Empty(), spadeCard);
            state = FieldState.AddCard(state, joker); // Spade + Joker

            // Assert
            Assert.IsFalse(state.IsBindingActive(rules), "Joker should not trigger binding (has no suit)");
        }

        /// <summary>
        /// Test: Joker breaks existing binding
        /// </summary>
        [Test]
        public void IsBindingActive_JokerBreaksBinding_ReturnsFalse()
        {
            // Arrange
            CardSO heart5 = TestHelpers.CreateCard(CardSO.Suit.Heart, 5);
            CardSO heart7 = TestHelpers.CreateCard(CardSO.Suit.Heart, 7);
            CardSO joker = TestHelpers.CreateJoker(isRed: true);
            GameRulesSO rules = TestHelpers.CreateGameRules(enableBind: true);

            // Act
            FieldState state = FieldState.AddCard(FieldState.Empty(), heart5);
            state = FieldState.AddCard(state, heart7); // Hearts binding active
            state = FieldState.AddCard(state, joker);  // Joker breaks binding

            // Assert
            Assert.IsFalse(state.IsBindingActive(rules), "Joker should break binding");
        }

        /// <summary>
        /// Test: Joker strength is 16 (beats rank 2)
        /// </summary>
        [Test]
        public void Strength_Joker_Returns16()
        {
            // Arrange
            CardSO joker = TestHelpers.CreateJoker(isRed: true);

            // Act
            FieldState state = FieldState.AddCard(FieldState.Empty(), joker);

            // Assert
            Assert.AreEqual(16, state.Strength, "Joker strength should be 16 (beats rank 2's 15)");
        }

        #endregion
    }
}

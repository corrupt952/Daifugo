using NUnit.Framework;
using Daifugo.AI;
using Daifugo.Core;
using Daifugo.Data;
using Daifugo.Tests.Helpers;
using UnityEngine;

namespace Daifugo.Tests.AI
{
    /// <summary>
    /// Tests for AIPlayerStrategy class
    /// Validates AI decision logic for card selection
    /// Pure C# class - no Unity dependencies required for testing
    /// </summary>
    public class AIPlayerStrategyTests
    {
        private AIPlayerStrategy strategy;
        private GameRulesSO gameRules;

        /// <summary>
        /// Sets up test fixtures before each test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            gameRules = TestHelpers.CreateGameRules(enable8Cut: true);
            strategy = new AIPlayerStrategy(gameRules);
        }

        /// <summary>
        /// Cleans up after each test
        /// </summary>
        [TearDown]
        public void Teardown()
        {
            strategy = null;
        }

        #region Empty Field Tests

        /// <summary>
        /// Test: AI selects weakest card when field is empty
        /// </summary>
        [Test]
        public void DecideAction_EmptyField_SelectsWeakestCard()
        {
            // Arrange
            CardSO card3 = TestHelpers.CreateCardByRank(3);   // Strength: 3
            CardSO card7 = TestHelpers.CreateCardByRank(7);   // Strength: 7
            CardSO card10 = TestHelpers.CreateCardByRank(10); // Strength: 10
            PlayerHandSO hand = TestHelpers.CreateHand(1, card3, card7, card10);
            FieldState fieldState = FieldState.Empty();

            // Act
            CardSO result = strategy.DecideAction(hand, fieldState);

            // Assert
            Assert.AreEqual(card3, result, "AI should select weakest card (3) on empty field");
        }

        /// <summary>
        /// Test: AI selects weakest card among all cards including Ace and 2
        /// </summary>
        [Test]
        public void DecideAction_EmptyFieldWithAceAnd2_SelectsWeakestCard()
        {
            // Arrange
            CardSO card3 = TestHelpers.CreateCardByRank(3);   // Strength: 3 (weakest)
            CardSO ace = TestHelpers.CreateCardByRank(1);     // Strength: 14
            CardSO two = TestHelpers.CreateCardByRank(2);     // Strength: 15 (strongest)
            PlayerHandSO hand = TestHelpers.CreateHand(1, card3, ace, two);
            FieldState fieldState = FieldState.Empty();

            // Act
            CardSO result = strategy.DecideAction(hand, fieldState);

            // Assert
            Assert.AreEqual(card3, result, "AI should select 3 (weakest) even with Ace and 2");
        }

        #endregion

        #region Field Card Tests

        /// <summary>
        /// Test: AI selects weakest playable card when field has card
        /// </summary>
        [Test]
        public void DecideAction_WithFieldCard_SelectsWeakestPlayableCard()
        {
            // Arrange
            CardSO card3 = TestHelpers.CreateCardByRank(3);   // Strength: 3 (not playable)
            CardSO card7 = TestHelpers.CreateCardByRank(7);   // Strength: 7 (playable, weakest)
            CardSO card10 = TestHelpers.CreateCardByRank(10); // Strength: 10 (playable)
            PlayerHandSO hand = TestHelpers.CreateHand(1, card3, card7, card10);
            FieldState fieldState = FieldState.AddCard(FieldState.Empty(), TestHelpers.CreateCardByRank(5)); // Field strength: 5

            // Act
            CardSO result = strategy.DecideAction(hand, fieldState);

            // Assert
            Assert.AreEqual(card7, result, "AI should select weakest playable card (7)");
        }

        /// <summary>
        /// Test: AI returns null when no cards are playable
        /// </summary>
        [Test]
        public void DecideAction_NoPlayableCards_ReturnsNull()
        {
            // Arrange
            CardSO card3 = TestHelpers.CreateCardByRank(3);   // Strength: 3
            CardSO card5 = TestHelpers.CreateCardByRank(5);   // Strength: 5
            PlayerHandSO hand = TestHelpers.CreateHand(1, card3, card5);
            FieldState fieldState = FieldState.AddCard(FieldState.Empty(), TestHelpers.CreateCardByRank(13)); // Field strength: 13 (King)

            // Act
            CardSO result = strategy.DecideAction(hand, fieldState);

            // Assert
            Assert.IsNull(result, "AI should return null when no cards are playable");
        }

        /// <summary>
        /// Test: AI can play Ace over King
        /// </summary>
        [Test]
        public void DecideAction_AceOverKing_SelectsAce()
        {
            // Arrange
            CardSO ace = TestHelpers.CreateCardByRank(1);    // Strength: 14
            PlayerHandSO hand = TestHelpers.CreateHand(1, ace);
            FieldState fieldState = FieldState.AddCard(FieldState.Empty(), TestHelpers.CreateCardByRank(13)); // King, strength: 13

            // Act
            CardSO result = strategy.DecideAction(hand, fieldState);

            // Assert
            Assert.AreEqual(ace, result, "AI should play Ace over King");
        }

        /// <summary>
        /// Test: AI can play 2 over Ace
        /// </summary>
        [Test]
        public void DecideAction_TwoOverAce_SelectsTwo()
        {
            // Arrange
            CardSO two = TestHelpers.CreateCardByRank(2);    // Strength: 15 (strongest)
            PlayerHandSO hand = TestHelpers.CreateHand(1, two);
            FieldState fieldState = FieldState.AddCard(FieldState.Empty(), TestHelpers.CreateCardByRank(1)); // Ace, strength: 14

            // Act
            CardSO result = strategy.DecideAction(hand, fieldState);

            // Assert
            Assert.AreEqual(two, result, "AI should play 2 over Ace");
        }

        #endregion

        #region Strategy Tests

        /// <summary>
        /// Test: AI prefers to conserve strong cards
        /// </summary>
        [Test]
        public void DecideAction_MultiplePlayableCards_ConservesStrongCards()
        {
            // Arrange
            CardSO card7 = TestHelpers.CreateCardByRank(7);   // Strength: 7 (weakest playable)
            CardSO card10 = TestHelpers.CreateCardByRank(10); // Strength: 10
            CardSO ace = TestHelpers.CreateCardByRank(1);     // Strength: 14
            CardSO two = TestHelpers.CreateCardByRank(2);     // Strength: 15 (strongest)
            PlayerHandSO hand = TestHelpers.CreateHand(1, card7, card10, ace, two);
            FieldState fieldState = FieldState.AddCard(FieldState.Empty(), TestHelpers.CreateCardByRank(5)); // Field strength: 5

            // Act
            CardSO result = strategy.DecideAction(hand, fieldState);

            // Assert
            Assert.AreEqual(card7, result, "AI should play weakest card (7) to conserve strong cards");
            Assert.AreNotEqual(two, result, "AI should not waste strongest card (2)");
            Assert.AreNotEqual(ace, result, "AI should not waste Ace");
        }

        #endregion

        #region Null Safety Tests

        /// <summary>
        /// Test: Returns null when hand is null
        /// </summary>
        [Test]
        public void DecideAction_NullHand_ReturnsNull()
        {
            // Arrange
            PlayerHandSO hand = null;
            FieldState fieldState = FieldState.Empty();

            // Act
            CardSO result = strategy.DecideAction(hand, fieldState);

            // Assert
            Assert.IsNull(result, "Should handle null hand gracefully");
        }

        /// <summary>
        /// Test: Works correctly with null field card (empty field)
        /// </summary>
        [Test]
        public void DecideAction_NullFieldCard_SelectsWeakestCard()
        {
            // Arrange
            CardSO card5 = TestHelpers.CreateCardByRank(5);
            CardSO card9 = TestHelpers.CreateCardByRank(9);
            PlayerHandSO hand = TestHelpers.CreateHand(1, card5, card9);
            FieldState fieldState = FieldState.Empty();

            // Act
            CardSO result = strategy.DecideAction(hand, fieldState);

            // Assert
            Assert.AreEqual(card5, result, "Should select weakest card with empty field");
        }

        #endregion

        #region Edge Cases

        /// <summary>
        /// Test: AI with single card plays it if possible
        /// </summary>
        [Test]
        public void DecideAction_SinglePlayableCard_SelectsThatCard()
        {
            // Arrange
            CardSO card7 = TestHelpers.CreateCardByRank(7);
            PlayerHandSO hand = TestHelpers.CreateHand(1, card7);
            FieldState fieldState = FieldState.AddCard(FieldState.Empty(), TestHelpers.CreateCardByRank(5));

            // Act
            CardSO result = strategy.DecideAction(hand, fieldState);

            // Assert
            Assert.AreEqual(card7, result, "AI should play the only playable card");
        }

        /// <summary>
        /// Test: AI with single unplayable card returns null
        /// </summary>
        [Test]
        public void DecideAction_SingleUnplayableCard_ReturnsNull()
        {
            // Arrange
            CardSO card3 = TestHelpers.CreateCardByRank(3);
            PlayerHandSO hand = TestHelpers.CreateHand(1, card3);
            FieldState fieldState = FieldState.AddCard(FieldState.Empty(), TestHelpers.CreateCardByRank(10));

            // Act
            CardSO result = strategy.DecideAction(hand, fieldState);

            // Assert
            Assert.IsNull(result, "AI should return null when only card is not playable");
        }

        #endregion
    }
}

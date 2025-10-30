using NUnit.Framework;
using Daifugo.Core;
using Daifugo.Data;
using Daifugo.Tests.Helpers;
using UnityEngine;

namespace Daifugo.Tests.Core
{
    /// <summary>
    /// Tests for RuleValidator class
    /// Validates card play rules according to Daifugo game logic
    /// </summary>
    public class RuleValidatorTests
    {
        private RuleValidator ruleValidator;

        /// <summary>
        /// Sets up test fixtures before each test
        /// Creates a GameObject with RuleValidator component
        /// </summary>
        [SetUp]
        public void Setup()
        {
            GameObject gameObject = new GameObject("RuleValidator");
            ruleValidator = gameObject.AddComponent<RuleValidator>();
        }

        /// <summary>
        /// Cleans up after each test
        /// </summary>
        [TearDown]
        public void Teardown()
        {
            if (ruleValidator != null)
            {
                Object.DestroyImmediate(ruleValidator.gameObject);
            }
        }

        #region CanPlayCard Tests

        /// <summary>
        /// Test: Any card can be played on empty field
        /// </summary>
        [Test]
        public void CanPlayCard_OnEmptyField_ReturnsTrue()
        {
            // Arrange
            CardSO anyCard = TestHelpers.CreateCardByRank(5);
            CardSO emptyField = null;

            // Act
            bool result = ruleValidator.CanPlayCard(anyCard, emptyField);

            // Assert
            Assert.IsTrue(result, "Any card should be playable on empty field");
        }

        /// <summary>
        /// Test: All cards can be played on empty field
        /// </summary>
        [Test]
        public void CanPlayCard_AllRanksOnEmptyField_ReturnsTrue()
        {
            // Arrange - Test all card ranks (1-13)
            CardSO emptyField = null;

            for (int rank = 1; rank <= 13; rank++)
            {
                CardSO card = TestHelpers.CreateCardByRank(rank);

                // Act
                bool result = ruleValidator.CanPlayCard(card, emptyField);

                // Assert
                Assert.IsTrue(result, $"Card rank {rank} should be playable on empty field");
            }
        }

        /// <summary>
        /// Test: Stronger card can be played on existing field
        /// </summary>
        [Test]
        public void CanPlayCard_StrongerCard_ReturnsTrue()
        {
            // Arrange
            CardSO weakCard = TestHelpers.CreateCardByRank(3);  // Strength: 3
            CardSO strongCard = TestHelpers.CreateCardByRank(7); // Strength: 7

            // Act
            bool result = ruleValidator.CanPlayCard(strongCard, weakCard);

            // Assert
            Assert.IsTrue(result, "Stronger card should be playable");
        }

        /// <summary>
        /// Test: Weaker card cannot be played on existing field
        /// </summary>
        [Test]
        public void CanPlayCard_WeakerCard_ReturnsFalse()
        {
            // Arrange
            CardSO weakCard = TestHelpers.CreateCardByRank(3);   // Strength: 3
            CardSO strongCard = TestHelpers.CreateCardByRank(10); // Strength: 10

            // Act
            bool result = ruleValidator.CanPlayCard(weakCard, strongCard);

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
            CardSO card1 = TestHelpers.CreateCardByRank(7);
            CardSO card2 = TestHelpers.CreateCardByRank(7);

            // Act
            bool result = ruleValidator.CanPlayCard(card2, card1);

            // Assert
            Assert.IsFalse(result, "Equal strength card should not be playable");
        }

        /// <summary>
        /// Test: Ace (rank 1) has strength 14
        /// </summary>
        [Test]
        public void CanPlayCard_Ace_HasCorrectStrength()
        {
            // Arrange
            CardSO ace = TestHelpers.CreateCardByRank(1);   // Strength: 14
            CardSO king = TestHelpers.CreateCardByRank(13); // Strength: 13

            // Act
            bool aceBeatsKing = ruleValidator.CanPlayCard(ace, king);
            bool kingBeatsAce = ruleValidator.CanPlayCard(king, ace);

            // Assert
            Assert.IsTrue(aceBeatsKing, "Ace should beat King");
            Assert.IsFalse(kingBeatsAce, "King should not beat Ace");
        }

        /// <summary>
        /// Test: 2 has highest strength (15)
        /// </summary>
        [Test]
        public void CanPlayCard_Two_HasHighestStrength()
        {
            // Arrange
            CardSO two = TestHelpers.CreateCardByRank(2);  // Strength: 15
            CardSO ace = TestHelpers.CreateCardByRank(1);  // Strength: 14

            // Act
            bool twoBeatsAce = ruleValidator.CanPlayCard(two, ace);
            bool aceBeatsTwo = ruleValidator.CanPlayCard(ace, two);

            // Assert
            Assert.IsTrue(twoBeatsAce, "2 should beat Ace");
            Assert.IsFalse(aceBeatsTwo, "Ace should not beat 2");
        }

        /// <summary>
        /// Test: Complete strength hierarchy (3 to 2)
        /// </summary>
        [Test]
        public void CanPlayCard_StrengthHierarchy_IsCorrect()
        {
            // Arrange - Card hierarchy: 2 > A > K > Q > ... > 4 > 3
            int[] rankOrder = { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 1, 2 }; // Weakest to strongest

            for (int i = 0; i < rankOrder.Length - 1; i++)
            {
                CardSO weakerCard = TestHelpers.CreateCardByRank(rankOrder[i]);
                CardSO strongerCard = TestHelpers.CreateCardByRank(rankOrder[i + 1]);

                // Act
                bool strongerBeatsWeaker = ruleValidator.CanPlayCard(strongerCard, weakerCard);
                bool weakerBeatsStronger = ruleValidator.CanPlayCard(weakerCard, strongerCard);

                // Assert
                Assert.IsTrue(strongerBeatsWeaker, $"Rank {rankOrder[i + 1]} should beat rank {rankOrder[i]}");
                Assert.IsFalse(weakerBeatsStronger, $"Rank {rankOrder[i]} should not beat rank {rankOrder[i + 1]}");
            }
        }

        /// <summary>
        /// Test: Null card parameter returns false
        /// </summary>
        [Test]
        public void CanPlayCard_NullCard_ReturnsFalse()
        {
            // Arrange
            CardSO nullCard = null;
            CardSO fieldCard = TestHelpers.CreateCardByRank(5);

            // Act
            bool result = ruleValidator.CanPlayCard(nullCard, fieldCard);

            // Assert
            Assert.IsFalse(result, "Null card should not be playable");
        }

        #endregion

        #region HasPlayableCards Tests

        /// <summary>
        /// Test: Empty hand has no playable cards
        /// </summary>
        [Test]
        public void HasPlayableCards_EmptyHand_ReturnsFalse()
        {
            // Arrange
            PlayerHandSO emptyHand = TestHelpers.CreateHand(0);
            CardSO fieldCard = TestHelpers.CreateCardByRank(5);

            // Act
            bool result = ruleValidator.HasPlayableCards(emptyHand, fieldCard);

            // Assert
            Assert.IsFalse(result, "Empty hand should have no playable cards");
        }

        /// <summary>
        /// Test: Hand with stronger cards has playable cards
        /// </summary>
        [Test]
        public void HasPlayableCards_WithStrongerCards_ReturnsTrue()
        {
            // Arrange
            CardSO card1 = TestHelpers.CreateCardByRank(3);
            CardSO card2 = TestHelpers.CreateCardByRank(7);
            PlayerHandSO hand = TestHelpers.CreateHand(0, card1, card2);
            CardSO fieldCard = TestHelpers.CreateCardByRank(5); // Weaker than card2

            // Act
            bool result = ruleValidator.HasPlayableCards(hand, fieldCard);

            // Assert
            Assert.IsTrue(result, "Hand with stronger cards should have playable cards");
        }

        /// <summary>
        /// Test: Hand with only weaker cards has no playable cards
        /// </summary>
        [Test]
        public void HasPlayableCards_OnlyWeakerCards_ReturnsFalse()
        {
            // Arrange
            CardSO card1 = TestHelpers.CreateCardByRank(3);
            CardSO card2 = TestHelpers.CreateCardByRank(4);
            PlayerHandSO hand = TestHelpers.CreateHand(0, card1, card2);
            CardSO fieldCard = TestHelpers.CreateCardByRank(10); // Stronger than all

            // Act
            bool result = ruleValidator.HasPlayableCards(hand, fieldCard);

            // Assert
            Assert.IsFalse(result, "Hand with only weaker cards should have no playable cards");
        }

        /// <summary>
        /// Test: Any hand has playable cards on empty field
        /// </summary>
        [Test]
        public void HasPlayableCards_OnEmptyField_ReturnsTrue()
        {
            // Arrange
            CardSO card = TestHelpers.CreateCardByRank(5);
            PlayerHandSO hand = TestHelpers.CreateHand(0, card);
            CardSO emptyField = null;

            // Act
            bool result = ruleValidator.HasPlayableCards(hand, emptyField);

            // Assert
            Assert.IsTrue(result, "Non-empty hand should have playable cards on empty field");
        }

        /// <summary>
        /// Test: Null hand parameter returns false
        /// </summary>
        [Test]
        public void HasPlayableCards_NullHand_ReturnsFalse()
        {
            // Arrange
            PlayerHandSO nullHand = null;
            CardSO fieldCard = TestHelpers.CreateCardByRank(5);

            // Act
            bool result = ruleValidator.HasPlayableCards(nullHand, fieldCard);

            // Assert
            Assert.IsFalse(result, "Null hand should have no playable cards");
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
            CardSO card = TestHelpers.CreateCardByRank(5);
            PlayerHandSO hand = TestHelpers.CreateHand(0, card);

            // Act
            bool result = ruleValidator.IsCardInHand(card, hand);

            // Assert
            Assert.IsTrue(result, "Card in hand should return true");
        }

        /// <summary>
        /// Test: Card not in hand returns false
        /// </summary>
        [Test]
        public void IsCardInHand_CardNotExists_ReturnsFalse()
        {
            // Arrange
            CardSO cardInHand = TestHelpers.CreateCardByRank(5);
            CardSO cardNotInHand = TestHelpers.CreateCardByRank(7);
            PlayerHandSO hand = TestHelpers.CreateHand(0, cardInHand);

            // Act
            bool result = ruleValidator.IsCardInHand(cardNotInHand, hand);

            // Assert
            Assert.IsFalse(result, "Card not in hand should return false");
        }

        /// <summary>
        /// Test: Null card parameter returns false
        /// </summary>
        [Test]
        public void IsCardInHand_NullCard_ReturnsFalse()
        {
            // Arrange
            CardSO nullCard = null;
            CardSO someCard = TestHelpers.CreateCardByRank(5);
            PlayerHandSO hand = TestHelpers.CreateHand(0, someCard);

            // Act
            bool result = ruleValidator.IsCardInHand(nullCard, hand);

            // Assert
            Assert.IsFalse(result, "Null card should return false");
        }

        /// <summary>
        /// Test: Null hand parameter returns false
        /// </summary>
        [Test]
        public void IsCardInHand_NullHand_ReturnsFalse()
        {
            // Arrange
            CardSO card = TestHelpers.CreateCardByRank(5);
            PlayerHandSO nullHand = null;

            // Act
            bool result = ruleValidator.IsCardInHand(card, nullHand);

            // Assert
            Assert.IsFalse(result, "Null hand should return false");
        }

        #endregion

        #region GetFieldStrength Tests

        /// <summary>
        /// Test: Empty field has strength 0
        /// </summary>
        [Test]
        public void GetFieldStrength_EmptyField_ReturnsZero()
        {
            // Arrange
            CardSO emptyField = null;

            // Act
            int strength = ruleValidator.GetFieldStrength(emptyField);

            // Assert
            Assert.AreEqual(0, strength, "Empty field should have strength 0");
        }

        /// <summary>
        /// Test: Field card returns correct strength
        /// </summary>
        [Test]
        public void GetFieldStrength_WithCard_ReturnsCardStrength()
        {
            // Arrange
            CardSO card = TestHelpers.CreateCardByRank(7);
            int expectedStrength = card.GetStrength();

            // Act
            int actualStrength = ruleValidator.GetFieldStrength(card);

            // Assert
            Assert.AreEqual(expectedStrength, actualStrength, "Field strength should match card strength");
        }

        #endregion
    }
}

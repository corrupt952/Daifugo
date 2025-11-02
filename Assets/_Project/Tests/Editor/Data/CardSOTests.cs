using NUnit.Framework;
using Daifugo.Data;
using Daifugo.Tests.Helpers;

namespace Daifugo.Tests.Data
{
    /// <summary>
    /// Tests for CardSO class
    /// Validates card strength calculation and Joker special behavior
    /// </summary>
    public class CardSOTests
    {
        #region Normal Card Strength Tests

        /// <summary>
        /// Test: Rank 2 has highest strength (15)
        /// </summary>
        [Test]
        public void GetStrength_Rank2_Returns15()
        {
            // Arrange
            CardSO card2 = TestHelpers.CreateCard(CardSO.Suit.Spade, 2);

            // Act
            int strength = card2.GetStrength();

            // Assert
            Assert.AreEqual(15, strength, "Rank 2 should have strength 15 (highest)");
        }

        /// <summary>
        /// Test: Ace (rank 1) has second highest strength (14)
        /// </summary>
        [Test]
        public void GetStrength_Ace_Returns14()
        {
            // Arrange
            CardSO ace = TestHelpers.CreateCard(CardSO.Suit.Heart, 1);

            // Act
            int strength = ace.GetStrength();

            // Assert
            Assert.AreEqual(14, strength, "Ace should have strength 14");
        }

        /// <summary>
        /// Test: King (rank 13) has strength 13
        /// </summary>
        [Test]
        public void GetStrength_King_Returns13()
        {
            // Arrange
            CardSO king = TestHelpers.CreateCard(CardSO.Suit.Diamond, 13);

            // Act
            int strength = king.GetStrength();

            // Assert
            Assert.AreEqual(13, strength, "King should have strength 13");
        }

        /// <summary>
        /// Test: Rank 3 has lowest normal strength (3)
        /// </summary>
        [Test]
        public void GetStrength_Rank3_Returns3()
        {
            // Arrange
            CardSO card3 = TestHelpers.CreateCard(CardSO.Suit.Club, 3);

            // Act
            int strength = card3.GetStrength();

            // Assert
            Assert.AreEqual(3, strength, "Rank 3 should have strength 3 (lowest normal)");
        }

        #endregion

        #region Joker Tests

        /// <summary>
        /// Test: Joker has highest strength (16, beats even rank 2)
        /// </summary>
        [Test]
        public void GetStrength_Joker_Returns16()
        {
            // Arrange
            CardSO joker = TestHelpers.CreateJoker(isRed: true);

            // Act
            int strength = joker.GetStrength();

            // Assert
            Assert.AreEqual(16, strength, "Joker should have strength 16 (beats rank 2's 15)");
        }

        /// <summary>
        /// Test: Joker IsJoker property returns true
        /// </summary>
        [Test]
        public void IsJoker_Joker_ReturnsTrue()
        {
            // Arrange
            CardSO joker = TestHelpers.CreateJoker(isRed: false);

            // Act & Assert
            Assert.IsTrue(joker.IsJoker, "Joker card should have IsJoker = true");
        }

        /// <summary>
        /// Test: Normal card IsJoker property returns false
        /// </summary>
        [Test]
        public void IsJoker_NormalCard_ReturnsFalse()
        {
            // Arrange
            CardSO normalCard = TestHelpers.CreateCard(CardSO.Suit.Spade, 5);

            // Act & Assert
            Assert.IsFalse(normalCard.IsJoker, "Normal card should have IsJoker = false");
        }

        /// <summary>
        /// Test: Red Joker and Black Joker have same strength
        /// </summary>
        [Test]
        public void GetStrength_RedAndBlackJoker_SameStrength()
        {
            // Arrange
            CardSO redJoker = TestHelpers.CreateJoker(isRed: true);
            CardSO blackJoker = TestHelpers.CreateJoker(isRed: false);

            // Act
            int redStrength = redJoker.GetStrength();
            int blackStrength = blackJoker.GetStrength();

            // Assert
            Assert.AreEqual(redStrength, blackStrength, "Red and Black Joker should have same strength");
        }

        #endregion

        #region Strength Comparison Tests

        /// <summary>
        /// Test: Joker beats rank 2
        /// </summary>
        [Test]
        public void GetStrength_JokerBeatsRank2()
        {
            // Arrange
            CardSO joker = TestHelpers.CreateJoker(isRed: true);
            CardSO card2 = TestHelpers.CreateCard(CardSO.Suit.Spade, 2);

            // Act
            int jokerStrength = joker.GetStrength();
            int rank2Strength = card2.GetStrength();

            // Assert
            Assert.Greater(jokerStrength, rank2Strength, "Joker should beat rank 2");
        }

        /// <summary>
        /// Test: Rank 2 beats Ace
        /// </summary>
        [Test]
        public void GetStrength_Rank2BeatsAce()
        {
            // Arrange
            CardSO card2 = TestHelpers.CreateCard(CardSO.Suit.Heart, 2);
            CardSO ace = TestHelpers.CreateCard(CardSO.Suit.Spade, 1);

            // Act
            int rank2Strength = card2.GetStrength();
            int aceStrength = ace.GetStrength();

            // Assert
            Assert.Greater(rank2Strength, aceStrength, "Rank 2 should beat Ace");
        }

        /// <summary>
        /// Test: Ace beats King
        /// </summary>
        [Test]
        public void GetStrength_AceBeatsKing()
        {
            // Arrange
            CardSO ace = TestHelpers.CreateCard(CardSO.Suit.Club, 1);
            CardSO king = TestHelpers.CreateCard(CardSO.Suit.Diamond, 13);

            // Act
            int aceStrength = ace.GetStrength();
            int kingStrength = king.GetStrength();

            // Assert
            Assert.Greater(aceStrength, kingStrength, "Ace should beat King");
        }

        #endregion

        #region Revolution Strength Tests (Phase 1.5)

        /// <summary>
        /// Test: Rank 3 has highest strength (15) in revolution
        /// </summary>
        [Test]
        public void GetStrength_Revolution_Rank3_Returns15()
        {
            // Arrange
            CardSO card3 = TestHelpers.CreateCard(CardSO.Suit.Spade, 3);

            // Act
            int strength = card3.GetStrength(isRevolution: true);

            // Assert
            Assert.AreEqual(15, strength, "Rank 3 should have strength 15 (highest in revolution)");
        }

        /// <summary>
        /// Test: Rank 2 has lowest strength (2) in revolution
        /// </summary>
        [Test]
        public void GetStrength_Revolution_Rank2_Returns2()
        {
            // Arrange
            CardSO card2 = TestHelpers.CreateCard(CardSO.Suit.Heart, 2);

            // Act
            int strength = card2.GetStrength(isRevolution: true);

            // Assert
            Assert.AreEqual(2, strength, "Rank 2 should have strength 2 (lowest in revolution, except Joker)");
        }

        /// <summary>
        /// Test: Ace has strength 3 in revolution
        /// </summary>
        [Test]
        public void GetStrength_Revolution_Ace_Returns3()
        {
            // Arrange
            CardSO ace = TestHelpers.CreateCard(CardSO.Suit.Diamond, 1);

            // Act
            int strength = ace.GetStrength(isRevolution: true);

            // Assert
            Assert.AreEqual(3, strength, "Ace should have strength 3 in revolution");
        }

        /// <summary>
        /// Test: King has strength 5 in revolution (18 - 13 = 5)
        /// </summary>
        [Test]
        public void GetStrength_Revolution_King_Returns5()
        {
            // Arrange
            CardSO king = TestHelpers.CreateCard(CardSO.Suit.Club, 13);

            // Act
            int strength = king.GetStrength(isRevolution: true);

            // Assert
            Assert.AreEqual(5, strength, "King should have strength 5 in revolution (18 - 13 = 5)");
        }

        /// <summary>
        /// Test: Rank 4 has strength 14 in revolution (18 - 4 = 14)
        /// </summary>
        [Test]
        public void GetStrength_Revolution_Rank4_Returns14()
        {
            // Arrange
            CardSO card4 = TestHelpers.CreateCard(CardSO.Suit.Spade, 4);

            // Act
            int strength = card4.GetStrength(isRevolution: true);

            // Assert
            Assert.AreEqual(14, strength, "Rank 4 should have strength 14 in revolution (18 - 4 = 14)");
        }

        /// <summary>
        /// Test: Joker still has highest strength (16) in revolution
        /// </summary>
        [Test]
        public void GetStrength_Revolution_Joker_Returns16()
        {
            // Arrange
            CardSO joker = TestHelpers.CreateJoker(isRed: true);

            // Act
            int strength = joker.GetStrength(isRevolution: true);

            // Assert
            Assert.AreEqual(16, strength, "Joker should still have strength 16 in revolution (always strongest)");
        }

        /// <summary>
        /// Test: In revolution, rank 3 beats rank 2
        /// </summary>
        [Test]
        public void GetStrength_Revolution_Rank3BeatsRank2()
        {
            // Arrange
            CardSO card3 = TestHelpers.CreateCard(CardSO.Suit.Spade, 3);
            CardSO card2 = TestHelpers.CreateCard(CardSO.Suit.Heart, 2);

            // Act
            int strength3 = card3.GetStrength(isRevolution: true);
            int strength2 = card2.GetStrength(isRevolution: true);

            // Assert
            Assert.Greater(strength3, strength2, "In revolution, rank 3 should beat rank 2 (15 > 2)");
        }

        /// <summary>
        /// Test: In revolution, rank 4 beats Ace
        /// </summary>
        [Test]
        public void GetStrength_Revolution_Rank4BeatsAce()
        {
            // Arrange
            CardSO card4 = TestHelpers.CreateCard(CardSO.Suit.Diamond, 4);
            CardSO ace = TestHelpers.CreateCard(CardSO.Suit.Club, 1);

            // Act
            int strength4 = card4.GetStrength(isRevolution: true);
            int strengthAce = ace.GetStrength(isRevolution: true);

            // Assert
            Assert.Greater(strength4, strengthAce, "In revolution, rank 4 should beat Ace (14 > 3)");
        }

        #endregion
    }
}

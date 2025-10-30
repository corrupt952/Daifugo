using NUnit.Framework;
using System.Collections.Generic;
using Daifugo.Core;
using Daifugo.Data;
using Daifugo.Tests.Helpers;
using UnityEngine;

namespace Daifugo.Tests.Core
{
    /// <summary>
    /// Tests for PlayableCardService class
    /// Validates playable card determination logic
    /// </summary>
    public class PlayableCardServiceTests
    {
        private PlayableCardService service;

        /// <summary>
        /// Sets up test fixtures before each test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            service = new PlayableCardService();
        }

        /// <summary>
        /// Cleans up after each test
        /// </summary>
        [TearDown]
        public void Teardown()
        {
            service = null;
        }

        #region Not Player's Turn Tests

        /// <summary>
        /// Test: Returns empty list when not the target player's turn
        /// </summary>
        [Test]
        public void GetPlayableCards_NotPlayerTurn_ReturnsEmptyList()
        {
            // Arrange
            CardSO card3 = TestHelpers.CreateCardByRank(3);
            CardSO card5 = TestHelpers.CreateCardByRank(5);
            CardSO card7 = TestHelpers.CreateCardByRank(7);
            PlayerHandSO hand = TestHelpers.CreateHand(0, card3, card5, card7);
            CardSO fieldCard = null;

            // Act
            List<CardSO> result = service.GetPlayableCardsForPlayer(
                currentPlayerID: 1,     // CPU 1's turn
                targetPlayerID: 0,      // Human player
                hand: hand,
                fieldCard: fieldCard
            );

            // Assert
            Assert.AreEqual(0, result.Count, "Should return empty list when not player's turn");
        }

        /// <summary>
        /// Test: Returns empty list when not the target player's turn (with field card)
        /// </summary>
        [Test]
        public void GetPlayableCards_NotPlayerTurnWithFieldCard_ReturnsEmptyList()
        {
            // Arrange
            CardSO card3 = TestHelpers.CreateCardByRank(3);
            CardSO card5 = TestHelpers.CreateCardByRank(5);
            CardSO card7 = TestHelpers.CreateCardByRank(7);
            PlayerHandSO hand = TestHelpers.CreateHand(0, card3, card5, card7);
            CardSO fieldCard = TestHelpers.CreateCardByRank(4);

            // Act
            List<CardSO> result = service.GetPlayableCardsForPlayer(
                currentPlayerID: 2,     // CPU 2's turn
                targetPlayerID: 0,      // Human player
                hand: hand,
                fieldCard: fieldCard
            );

            // Assert
            Assert.AreEqual(0, result.Count, "Should return empty list when not player's turn, even with playable cards");
        }

        #endregion

        #region Player's Turn - Empty Field Tests

        /// <summary>
        /// Test: Returns all cards when field is empty and it's player's turn
        /// </summary>
        [Test]
        public void GetPlayableCards_EmptyField_PlayerTurn_ReturnsAllCards()
        {
            // Arrange
            CardSO card3 = TestHelpers.CreateCardByRank(3);
            CardSO card5 = TestHelpers.CreateCardByRank(5);
            CardSO card7 = TestHelpers.CreateCardByRank(7);
            PlayerHandSO hand = TestHelpers.CreateHand(0, card3, card5, card7);
            CardSO fieldCard = null;

            // Act
            List<CardSO> result = service.GetPlayableCardsForPlayer(
                currentPlayerID: 0,     // Human player's turn
                targetPlayerID: 0,      // Human player
                hand: hand,
                fieldCard: fieldCard
            );

            // Assert
            Assert.AreEqual(3, result.Count, "All cards should be playable on empty field");
            Assert.Contains(card3, result);
            Assert.Contains(card5, result);
            Assert.Contains(card7, result);
        }

        #endregion

        #region Player's Turn - Field Card Tests

        /// <summary>
        /// Test: Returns only cards stronger than field card
        /// </summary>
        [Test]
        public void GetPlayableCards_WithFieldCard_PlayerTurn_ReturnsStrongerCards()
        {
            // Arrange
            CardSO card3 = TestHelpers.CreateCardByRank(3);  // Strength: 3
            CardSO card5 = TestHelpers.CreateCardByRank(5);  // Strength: 5
            CardSO card7 = TestHelpers.CreateCardByRank(7);  // Strength: 7
            PlayerHandSO hand = TestHelpers.CreateHand(0, card3, card5, card7);
            CardSO fieldCard = TestHelpers.CreateCardByRank(5);  // Field strength: 5

            // Act
            List<CardSO> result = service.GetPlayableCardsForPlayer(
                currentPlayerID: 0,     // Human player's turn
                targetPlayerID: 0,      // Human player
                hand: hand,
                fieldCard: fieldCard
            );

            // Assert
            Assert.AreEqual(1, result.Count, "Only cards stronger than field should be playable");
            Assert.Contains(card7, result, "Card 7 should be playable");
            Assert.IsFalse(result.Contains(card3), "Card 3 should not be playable");
            Assert.IsFalse(result.Contains(card5), "Card 5 (equal) should not be playable");
        }

        /// <summary>
        /// Test: Returns no cards when no cards are stronger than field
        /// </summary>
        [Test]
        public void GetPlayableCards_NoStrongerCards_PlayerTurn_ReturnsEmptyList()
        {
            // Arrange
            CardSO card3 = TestHelpers.CreateCardByRank(3);  // Strength: 3
            CardSO card5 = TestHelpers.CreateCardByRank(5);  // Strength: 5
            PlayerHandSO hand = TestHelpers.CreateHand(0, card3, card5);
            CardSO fieldCard = TestHelpers.CreateCardByRank(13);  // Field strength: 13 (King)

            // Act
            List<CardSO> result = service.GetPlayableCardsForPlayer(
                currentPlayerID: 0,     // Human player's turn
                targetPlayerID: 0,      // Human player
                hand: hand,
                fieldCard: fieldCard
            );

            // Assert
            Assert.AreEqual(0, result.Count, "Should return empty list when no cards are stronger");
        }

        /// <summary>
        /// Test: Ace (rank 1) beats King (rank 13) in strength
        /// </summary>
        [Test]
        public void GetPlayableCards_AceBeatsKing_PlayerTurn()
        {
            // Arrange
            CardSO aceCard = TestHelpers.CreateCardByRank(1);    // Strength: 14 (Ace)
            CardSO kingCard = TestHelpers.CreateCardByRank(13);  // Strength: 13 (King)
            PlayerHandSO hand = TestHelpers.CreateHand(0, aceCard);
            CardSO fieldCard = kingCard;

            // Act
            List<CardSO> result = service.GetPlayableCardsForPlayer(
                currentPlayerID: 0,
                targetPlayerID: 0,
                hand: hand,
                fieldCard: fieldCard
            );

            // Assert
            Assert.AreEqual(1, result.Count, "Ace should beat King");
            Assert.Contains(aceCard, result);
        }

        /// <summary>
        /// Test: 2 (rank 2) beats Ace (rank 1) in strength
        /// </summary>
        [Test]
        public void GetPlayableCards_TwoBeatsAce_PlayerTurn()
        {
            // Arrange
            CardSO twoCard = TestHelpers.CreateCardByRank(2);    // Strength: 15 (strongest)
            CardSO aceCard = TestHelpers.CreateCardByRank(1);    // Strength: 14
            PlayerHandSO hand = TestHelpers.CreateHand(0, twoCard);
            CardSO fieldCard = aceCard;

            // Act
            List<CardSO> result = service.GetPlayableCardsForPlayer(
                currentPlayerID: 0,
                targetPlayerID: 0,
                hand: hand,
                fieldCard: fieldCard
            );

            // Assert
            Assert.AreEqual(1, result.Count, "2 should beat Ace");
            Assert.Contains(twoCard, result);
        }

        #endregion

        #region Null Safety Tests

        /// <summary>
        /// Test: Returns empty list when hand is null
        /// </summary>
        [Test]
        public void GetPlayableCards_NullHand_ReturnsEmptyList()
        {
            // Arrange
            PlayerHandSO hand = null;
            CardSO fieldCard = null;

            // Act
            List<CardSO> result = service.GetPlayableCardsForPlayer(
                currentPlayerID: 0,
                targetPlayerID: 0,
                hand: hand,
                fieldCard: fieldCard
            );

            // Assert
            Assert.AreEqual(0, result.Count, "Should handle null hand gracefully");
        }

        #endregion

        #region CPU Player Tests

        /// <summary>
        /// Test: CPU player gets playable cards during their turn
        /// </summary>
        [Test]
        public void GetPlayableCards_CPUPlayer_TheirTurn_ReturnsPlayableCards()
        {
            // Arrange
            CardSO card5 = TestHelpers.CreateCardByRank(5);
            CardSO card7 = TestHelpers.CreateCardByRank(7);
            PlayerHandSO cpuHand = TestHelpers.CreateHand(1, card5, card7);
            CardSO fieldCard = TestHelpers.CreateCardByRank(4);

            // Act
            List<CardSO> result = service.GetPlayableCardsForPlayer(
                currentPlayerID: 1,     // CPU 1's turn
                targetPlayerID: 1,      // CPU 1
                hand: cpuHand,
                fieldCard: fieldCard
            );

            // Assert
            Assert.AreEqual(2, result.Count, "CPU should get playable cards during their turn");
            Assert.Contains(card5, result);
            Assert.Contains(card7, result);
        }

        /// <summary>
        /// Test: CPU player gets empty list when not their turn
        /// </summary>
        [Test]
        public void GetPlayableCards_CPUPlayer_NotTheirTurn_ReturnsEmptyList()
        {
            // Arrange
            CardSO card5 = TestHelpers.CreateCardByRank(5);
            CardSO card7 = TestHelpers.CreateCardByRank(7);
            PlayerHandSO cpuHand = TestHelpers.CreateHand(1, card5, card7);
            CardSO fieldCard = null;

            // Act
            List<CardSO> result = service.GetPlayableCardsForPlayer(
                currentPlayerID: 0,     // Human player's turn
                targetPlayerID: 1,      // CPU 1
                hand: cpuHand,
                fieldCard: fieldCard
            );

            // Assert
            Assert.AreEqual(0, result.Count, "CPU should get empty list when not their turn");
        }

        #endregion
    }
}

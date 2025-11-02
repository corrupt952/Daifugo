using NUnit.Framework;
using Daifugo.Events;
using Daifugo.Data;
using Daifugo.Core;
using Daifugo.Tests.Helpers;
using System.Collections.Generic;

namespace Daifugo.Tests.Events
{
    /// <summary>
    /// Tests for CardPlayedEventData structure
    /// Validates Cards list and Card property compatibility
    /// </summary>
    public class CardPlayedEventDataTests
    {
        /// <summary>
        /// Test: Card property returns first card from Cards list
        /// </summary>
        [Test]
        public void Card_WithMultipleCards_ReturnsFirstCard()
        {
            // Arrange
            CardSO card1 = TestHelpers.CreateCardByRank(3);
            CardSO card2 = TestHelpers.CreateCardByRank(5);
            var cards = new List<CardSO> { card1, card2 };

            var eventData = new CardPlayedEventData
            {
                Cards = cards,
                PlayerID = 0,
                FieldState = FieldState.Empty()
            };

            // Act
            CardSO result = eventData.Card;

            // Assert
            Assert.AreEqual(card1, result, "Card property should return first card");
        }

        /// <summary>
        /// Test: Card property returns card when Cards has single card
        /// </summary>
        [Test]
        public void Card_WithSingleCard_ReturnsThatCard()
        {
            // Arrange
            CardSO card = TestHelpers.CreateCardByRank(7);
            var cards = new List<CardSO> { card };

            var eventData = new CardPlayedEventData
            {
                Cards = cards,
                PlayerID = 0,
                FieldState = FieldState.Empty()
            };

            // Act
            CardSO result = eventData.Card;

            // Assert
            Assert.AreEqual(card, result, "Card property should return the single card");
        }

        /// <summary>
        /// Test: Card property returns null when Cards is null
        /// </summary>
        [Test]
        public void Card_WithNullCards_ReturnsNull()
        {
            // Arrange
            var eventData = new CardPlayedEventData
            {
                Cards = null,
                PlayerID = 0,
                FieldState = FieldState.Empty()
            };

            // Act
            CardSO result = eventData.Card;

            // Assert
            Assert.IsNull(result, "Card property should return null when Cards is null");
        }

        /// <summary>
        /// Test: Card property returns null when Cards is empty list
        /// </summary>
        [Test]
        public void Card_WithEmptyCards_ReturnsNull()
        {
            // Arrange
            var eventData = new CardPlayedEventData
            {
                Cards = new List<CardSO>(),
                PlayerID = 0,
                FieldState = FieldState.Empty()
            };

            // Act
            CardSO result = eventData.Card;

            // Assert
            Assert.IsNull(result, "Card property should return null when Cards is empty");
        }

        /// <summary>
        /// Test: Cards list preserves all cards
        /// </summary>
        [Test]
        public void Cards_WithMultipleCards_PreservesAllCards()
        {
            // Arrange
            CardSO card1 = TestHelpers.CreateCardByRank(3);
            CardSO card2 = TestHelpers.CreateCardByRank(3);
            CardSO card3 = TestHelpers.CreateCardByRank(3);
            var cards = new List<CardSO> { card1, card2, card3 };

            var eventData = new CardPlayedEventData
            {
                Cards = cards,
                PlayerID = 0,
                FieldState = FieldState.Empty()
            };

            // Act & Assert
            Assert.AreEqual(3, eventData.Cards.Count, "Cards should preserve all 3 cards");
            Assert.AreEqual(card1, eventData.Cards[0]);
            Assert.AreEqual(card2, eventData.Cards[1]);
            Assert.AreEqual(card3, eventData.Cards[2]);
        }
    }
}

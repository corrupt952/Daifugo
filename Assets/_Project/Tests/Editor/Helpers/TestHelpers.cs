using Daifugo.Data;
using UnityEngine;

namespace Daifugo.Tests.Helpers
{
    /// <summary>
    /// Helper utilities for creating test data
    /// Provides factory methods for ScriptableObject instances used in tests
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Creates a CardSO instance for testing
        /// </summary>
        /// <param name="suit">Card suit</param>
        /// <param name="rank">Card rank (1-13)</param>
        /// <returns>CardSO instance with specified properties</returns>
        public static CardSO CreateCard(CardSO.Suit suit, int rank)
        {
            var card = ScriptableObject.CreateInstance<CardSO>();

            // Use reflection to set private fields since CardSO doesn't have public setters
            var suitField = typeof(CardSO).GetField("suit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var rankField = typeof(CardSO).GetField("rank", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            suitField?.SetValue(card, suit);
            rankField?.SetValue(card, rank);

            return card;
        }

        /// <summary>
        /// Creates a PlayerHandSO instance for testing
        /// </summary>
        /// <param name="playerID">Player ID</param>
        /// <param name="cards">Initial cards in hand (optional)</param>
        /// <returns>PlayerHandSO instance</returns>
        public static PlayerHandSO CreateHand(int playerID, params CardSO[] cards)
        {
            var hand = ScriptableObject.CreateInstance<PlayerHandSO>();

            // Use reflection to set private fields
            var playerIDField = typeof(PlayerHandSO).GetField("playerID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playerIDField?.SetValue(hand, playerID);

            // Initialize hand
            hand.Initialize();

            // Add cards if provided
            foreach (var card in cards)
            {
                hand.AddCard(card);
            }

            return hand;
        }

        /// <summary>
        /// Creates a card with specific rank for concise test setup
        /// Uses Spade suit by default
        /// </summary>
        /// <param name="rank">Card rank (1-13)</param>
        /// <returns>CardSO instance</returns>
        public static CardSO CreateCardByRank(int rank)
        {
            return CreateCard(CardSO.Suit.Spade, rank);
        }
    }
}

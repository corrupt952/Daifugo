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

        /// <summary>
        /// Creates a Joker card for testing
        /// </summary>
        /// <param name="isRed">True for red joker, false for black joker</param>
        /// <returns>CardSO instance with IsJoker = true</returns>
        public static CardSO CreateJoker(bool isRed)
        {
            var joker = ScriptableObject.CreateInstance<CardSO>();

            // Use reflection to set private fields
            var suitField = typeof(CardSO).GetField("suit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var rankField = typeof(CardSO).GetField("rank", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var isJokerField = typeof(CardSO).GetField("isJoker", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Joker has no meaningful suit (use Spade by convention)
            suitField?.SetValue(joker, CardSO.Suit.Spade);
            // Joker has rank 0 (special value)
            rankField?.SetValue(joker, 0);
            // Set isJoker flag
            isJokerField?.SetValue(joker, true);

            return joker;
        }

        /// <summary>
        /// Creates a GameRulesSO instance for testing
        /// </summary>
        /// <param name="enableRevolution">革命ルールを有効にするか</param>
        /// <param name="enable8Cut">8切りルールを有効にするか</param>
        /// <param name="enableBind">縛りルールを有効にするか</param>
        /// <param name="enable11Back">11バックルールを有効にするか</param>
        /// <param name="enableSpade3Return">スペ3返しルールを有効にするか</param>
        /// <returns>GameRulesSO instance</returns>
        public static GameRulesSO CreateGameRules(
            bool enableRevolution = false,
            bool enable8Cut = true,
            bool enableBind = false,
            bool enable11Back = false,
            bool enableSpade3Return = false)
        {
            var rules = ScriptableObject.CreateInstance<GameRulesSO>();

            // Use reflection to set private fields
            var revolutionField = typeof(GameRulesSO).GetField("enableRevolution", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var cut8Field = typeof(GameRulesSO).GetField("enable8Cut", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var bindField = typeof(GameRulesSO).GetField("enableBind", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var back11Field = typeof(GameRulesSO).GetField("enable11Back", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var spade3ReturnField = typeof(GameRulesSO).GetField("enableSpade3Return", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            revolutionField?.SetValue(rules, enableRevolution);
            cut8Field?.SetValue(rules, enable8Cut);
            bindField?.SetValue(rules, enableBind);
            back11Field?.SetValue(rules, enable11Back);
            spade3ReturnField?.SetValue(rules, enableSpade3Return);

            return rules;
        }
    }
}

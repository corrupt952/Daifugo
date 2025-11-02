using System.Collections.Generic;
using System.Linq;
using Daifugo.Data;

namespace Daifugo.Core
{
    /// <summary>
    /// Detects play pattern from card combination
    /// Pure C# class for testability
    /// Phase 1.5: Supports single, pairs, triples, quadruples, and sequences
    /// </summary>
    public class PlayPatternDetector
    {
        /// <summary>
        /// Detects pattern from cards
        /// Priority: Same rank → Sequence → Invalid
        /// </summary>
        /// <param name="cards">Cards to analyze</param>
        /// <returns>Detected play pattern</returns>
        public PlayPattern DetectPattern(List<CardSO> cards)
        {
            if (cards == null || cards.Count == 0)
                return PlayPattern.Invalid;

            // Single card
            if (cards.Count == 1)
                return PlayPattern.Single;

            // Same rank check (Pair/Triple/Quadruple)
            if (IsAllSameRank(cards))
            {
                return cards.Count switch
                {
                    2 => PlayPattern.Pair,
                    3 => PlayPattern.Triple,
                    4 => PlayPattern.Quadruple,
                    _ => PlayPattern.Invalid  // 5+ not supported in Phase 1.5
                };
            }

            // Sequence check
            if (IsSequence(cards))
                return PlayPattern.Sequence;

            return PlayPattern.Invalid;
        }

        /// <summary>
        /// Checks if all cards have same rank
        /// </summary>
        /// <param name="cards">Cards to check</param>
        /// <returns>True if all cards have same rank (excluding Jokers)</returns>
        private bool IsAllSameRank(List<CardSO> cards)
        {
            if (cards.Count == 0) return false;

            // Jokers cannot form same rank patterns
            if (cards.Any(c => c.IsJoker)) return false;

            int firstRank = cards[0].Rank;
            return cards.All(c => c.Rank == firstRank);
        }

        /// <summary>
        /// Checks if cards form a sequence
        /// Requirements: 3+ cards, same suit, consecutive ranks
        /// Phase 1.5: No Joker, no wrap-around (K-A-2)
        /// </summary>
        /// <param name="cards">Cards to check</param>
        /// <returns>True if cards form a valid sequence</returns>
        public bool IsSequence(List<CardSO> cards)
        {
            if (cards.Count < 3) return false;

            // No Joker in sequence (Phase 1.5)
            if (cards.Any(c => c.IsJoker)) return false;

            // All same suit
            var suit = cards[0].CardSuit;
            if (cards.Any(c => c.CardSuit != suit)) return false;

            // Sort by rank
            var sorted = cards.OrderBy(c => c.Rank).ToList();

            // Check consecutive
            for (int i = 1; i < sorted.Count; i++)
            {
                if (sorted[i].Rank != sorted[i - 1].Rank + 1)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets sequence strength
        /// Normal: Max card strength (strongest card determines strength)
        /// Revolution: Min card strength (weakest card determines strength, but still use > operator for comparison)
        /// </summary>
        /// <param name="cards">Cards in sequence</param>
        /// <param name="isRevolution">Whether revolution is active</param>
        /// <returns>Sequence strength value</returns>
        public int GetSequenceStrength(List<CardSO> cards, bool isRevolution)
        {
            if (cards.Count == 0) return 0;

            if (isRevolution)
            {
                // Revolution: Compare by weakest card (min strength value)
                // Even though we compare by min, we still use > operator
                // because GetStrength(true) already returns revolution-adjusted values
                return cards.Min(c => c.GetStrength(true));
            }
            else
            {
                // Normal: Compare by strongest card (max strength value)
                return cards.Max(c => c.GetStrength(false));
            }
        }
    }
}

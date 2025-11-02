using System.Collections.Generic;
using System.Linq;
using Daifugo.Core;
using Daifugo.Data;

namespace Daifugo.AI
{
    /// <summary>
    /// AI decision logic for computer players
    /// Pure C# class (no MonoBehaviour) for testability
    /// Implements simple greedy strategy: play weakest valid card
    /// Phase 1.5: Supports multiple cards (pairs, triples, sequences)
    /// </summary>
    public class AIPlayerStrategy
    {
        private readonly PlayableCardsCalculator calculator;
        private readonly PlayPatternDetector patternDetector;
        private readonly GameRulesSO gameRules;

        /// <summary>
        /// Creates a new AIPlayerStrategy instance
        /// </summary>
        /// <param name="gameRules">Game rules configuration</param>
        public AIPlayerStrategy(GameRulesSO gameRules)
        {
            calculator = new PlayableCardsCalculator();
            patternDetector = new PlayPatternDetector();
            this.gameRules = gameRules;
        }

        /// <summary>
        /// Decides which card to play or whether to pass
        /// </summary>
        /// <param name="hand">AI player's hand</param>
        /// <param name="fieldState">Current field state (includes card history)</param>
        /// <returns>Card to play, or null to pass</returns>
        public CardSO DecideAction(PlayerHandSO hand, FieldState fieldState)
        {
            // Validate inputs
            if (hand == null)
            {
                return null;
            }

            // 1. Get playable cards based on field state (includes binding check)
            var playableCards = calculator.GetPlayableCards(hand, fieldState, gameRules);

            // 2. If no playable cards, return null (pass)
            if (playableCards.Count == 0)
            {
                return null;
            }

            // 3. Select weakest playable card (greedy strategy)
            return playableCards
                .OrderBy(c => c.GetStrength())
                .First();
        }

        // ========== Phase 1.5: Multiple Cards Decision ==========

        /// <summary>
        /// Decides which multiple cards to play when field requires multiple cards
        /// Phase 1.5: Simple greedy strategy - play weakest valid combination
        /// Avoids playing quadruple (revolution) to keep strategy simple
        /// </summary>
        /// <param name="hand">AI player's hand</param>
        /// <param name="fieldState">Current field state</param>
        /// <returns>List of cards to play, or null to pass</returns>
        public List<CardSO> DecideMultipleCardAction(PlayerHandSO hand, FieldState fieldState)
        {
            // Empty field: Phase 1.5 AI only plays single cards on empty field
            if (fieldState.IsEmpty)
                return null;

            // Get last play pattern and count
            PlayPattern lastPattern = fieldState.GetLastPlayPattern();
            int requiredCount = fieldState.GetLastPlayCount();

            // Decide based on pattern
            switch (lastPattern)
            {
                case PlayPattern.Pair:
                    return FindWeakestPlayablePair(hand, fieldState);

                case PlayPattern.Triple:
                    return FindWeakestPlayableTriple(hand, fieldState);

                case PlayPattern.Sequence:
                    return FindWeakestPlayableSequence(hand, fieldState, requiredCount);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Finds weakest playable pair in hand
        /// </summary>
        private List<CardSO> FindWeakestPlayablePair(PlayerHandSO hand, FieldState fieldState)
        {
            var pairs = FindPairsInHand(hand);
            bool isRevolution = fieldState.GetEffectiveRevolution();

            // Filter playable pairs (stronger than field)
            var playablePairs = pairs
                .Where(pair => pair[0].GetStrength(isRevolution) > fieldState.Strength)
                .OrderBy(pair => pair[0].GetStrength(isRevolution))
                .ToList();

            return playablePairs.Count > 0 ? playablePairs[0] : null;
        }

        /// <summary>
        /// Finds weakest playable triple in hand
        /// </summary>
        private List<CardSO> FindWeakestPlayableTriple(PlayerHandSO hand, FieldState fieldState)
        {
            var triples = FindTriplesInHand(hand);
            bool isRevolution = fieldState.GetEffectiveRevolution();

            // Filter playable triples (stronger than field)
            var playableTriples = triples
                .Where(triple => triple[0].GetStrength(isRevolution) > fieldState.Strength)
                .OrderBy(triple => triple[0].GetStrength(isRevolution))
                .ToList();

            return playableTriples.Count > 0 ? playableTriples[0] : null;
        }

        /// <summary>
        /// Finds weakest playable sequence in hand
        /// </summary>
        private List<CardSO> FindWeakestPlayableSequence(PlayerHandSO hand, FieldState fieldState, int requiredCount)
        {
            var sequences = FindSequencesInHand(hand, requiredCount);
            bool isRevolution = fieldState.GetEffectiveRevolution();

            int fieldStrength = patternDetector.GetSequenceStrength(
                fieldState.CurrentPlay.Value.Cards.ToList(),
                isRevolution
            );

            // Filter playable sequences (stronger than field)
            var playableSequences = sequences
                .Where(seq => patternDetector.GetSequenceStrength(seq, isRevolution) > fieldStrength)
                .OrderBy(seq => patternDetector.GetSequenceStrength(seq, isRevolution))
                .ToList();

            return playableSequences.Count > 0 ? playableSequences[0] : null;
        }

        /// <summary>
        /// Finds all pairs in hand
        /// Phase 1.5: Avoids quadruple by only returning first 2 cards of same rank
        /// </summary>
        private List<List<CardSO>> FindPairsInHand(PlayerHandSO hand)
        {
            var pairs = new List<List<CardSO>>();

            var groupedByRank = hand.Cards
                .Where(c => !c.IsJoker)
                .GroupBy(c => c.Rank)
                .Where(g => g.Count() >= 2);

            foreach (var group in groupedByRank)
            {
                // Take only first 2 cards (avoid quadruple)
                pairs.Add(group.Take(2).ToList());
            }

            return pairs;
        }

        /// <summary>
        /// Finds all triples in hand
        /// </summary>
        private List<List<CardSO>> FindTriplesInHand(PlayerHandSO hand)
        {
            var triples = new List<List<CardSO>>();

            var groupedByRank = hand.Cards
                .Where(c => !c.IsJoker)
                .GroupBy(c => c.Rank)
                .Where(g => g.Count() >= 3);

            foreach (var group in groupedByRank)
            {
                // Take only first 3 cards (avoid quadruple)
                triples.Add(group.Take(3).ToList());
            }

            return triples;
        }

        /// <summary>
        /// Finds all sequences of given length in hand
        /// Phase 1.5: Simple implementation - checks all possible consecutive sequences
        /// </summary>
        private List<List<CardSO>> FindSequencesInHand(PlayerHandSO hand, int length)
        {
            var sequences = new List<List<CardSO>>();

            // Group cards by suit
            var groupedBySuit = hand.Cards
                .Where(c => !c.IsJoker)
                .GroupBy(c => c.CardSuit);

            foreach (var suitGroup in groupedBySuit)
            {
                var sortedCards = suitGroup.OrderBy(c => c.Rank).ToList();

                // Find consecutive sequences
                for (int i = 0; i <= sortedCards.Count - length; i++)
                {
                    var potentialSequence = sortedCards.Skip(i).Take(length).ToList();

                    // Check if consecutive
                    bool isConsecutive = true;
                    for (int j = 1; j < potentialSequence.Count; j++)
                    {
                        if (potentialSequence[j].Rank != potentialSequence[j - 1].Rank + 1)
                        {
                            isConsecutive = false;
                            break;
                        }
                    }

                    if (isConsecutive)
                    {
                        sequences.Add(potentialSequence);
                    }
                }
            }

            return sequences;
        }
    }
}

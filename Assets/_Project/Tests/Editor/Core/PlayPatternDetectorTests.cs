using System.Collections.Generic;
using Daifugo.Core;
using Daifugo.Data;
using Daifugo.Tests.Helpers;
using NUnit.Framework;

namespace Daifugo.Tests.Core
{
    /// <summary>
    /// Tests for PlayPatternDetector
    /// Phase 1.5: Pattern detection and sequence strength calculation
    /// </summary>
    public class PlayPatternDetectorTests
    {
        private PlayPatternDetector detector;

        [SetUp]
        public void SetUp()
        {
            detector = new PlayPatternDetector();
        }

        // ========== Single Card Detection ==========

        [Test]
        public void DetectPattern_SingleCard_ReturnsSingle()
        {
            // Arrange
            var cards = new List<CardSO> { TestHelpers.CreateCard(CardSO.Suit.Spade, 5) };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Single, pattern);
        }

        // ========== Pair Detection ==========

        [Test]
        public void DetectPattern_Pair_ReturnsPair()
        {
            // Arrange
            var cards = new List<CardSO>
            {
                TestHelpers.CreateCard(CardSO.Suit.Spade, 5),
                TestHelpers.CreateCard(CardSO.Suit.Heart, 5)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Pair, pattern);
        }

        [Test]
        public void DetectPattern_PairDifferentSuits_ReturnsPair()
        {
            // Arrange
            var cards = new List<CardSO>
            {
                TestHelpers.CreateCard(CardSO.Suit.Diamond, 7),
                TestHelpers.CreateCard(CardSO.Suit.Club, 7)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Pair, pattern);
        }

        // ========== Triple Detection ==========

        [Test]
        public void DetectPattern_Triple_ReturnsTriple()
        {
            // Arrange
            var cards = new List<CardSO>
            {
                TestHelpers.CreateCard(CardSO.Suit.Spade, 5),
                TestHelpers.CreateCard(CardSO.Suit.Heart, 5),
                TestHelpers.CreateCard(CardSO.Suit.Diamond, 5)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Triple, pattern);
        }

        // ========== Quadruple Detection ==========

        [Test]
        public void DetectPattern_Quadruple_ReturnsQuadruple()
        {
            // Arrange
            var cards = new List<CardSO>
            {
                TestHelpers.CreateCard(CardSO.Suit.Spade, 5),
                TestHelpers.CreateCard(CardSO.Suit.Heart, 5),
                TestHelpers.CreateCard(CardSO.Suit.Diamond, 5),
                TestHelpers.CreateCard(CardSO.Suit.Club, 5)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Quadruple, pattern);
        }

        // ========== Sequence Detection ==========

        [Test]
        public void DetectPattern_Sequence3Cards_ReturnsSequence()
        {
            // Arrange
            var cards = new List<CardSO>
            {
                TestHelpers.CreateCard(CardSO.Suit.Spade, 3),
                TestHelpers.CreateCard(CardSO.Suit.Spade, 4),
                TestHelpers.CreateCard(CardSO.Suit.Spade, 5)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Sequence, pattern);
        }

        [Test]
        public void DetectPattern_Sequence5Cards_ReturnsSequence()
        {
            // Arrange
            var cards = new List<CardSO>
            {
                TestHelpers.CreateCard(CardSO.Suit.Heart, 7),
                TestHelpers.CreateCard(CardSO.Suit.Heart, 8),
                TestHelpers.CreateCard(CardSO.Suit.Heart, 9),
                TestHelpers.CreateCard(CardSO.Suit.Heart, 10),
                TestHelpers.CreateCard(CardSO.Suit.Heart, 11)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Sequence, pattern);
        }

        [Test]
        public void DetectPattern_SequenceNotConsecutive_ReturnsInvalid()
        {
            // Arrange: 3, 5, 7 - not consecutive
            var cards = new List<CardSO>
            {
                TestHelpers.CreateCard(CardSO.Suit.Spade, 3),
                TestHelpers.CreateCard(CardSO.Suit.Spade, 5),
                TestHelpers.CreateCard(CardSO.Suit.Spade, 7)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Invalid, pattern);
        }

        [Test]
        public void DetectPattern_SequenceDifferentSuits_ReturnsInvalid()
        {
            // Arrange: Same ranks but different suits
            var cards = new List<CardSO>
            {
                TestHelpers.CreateCard(CardSO.Suit.Spade, 3),
                TestHelpers.CreateCard(CardSO.Suit.Heart, 4),
                TestHelpers.CreateCard(CardSO.Suit.Diamond, 5)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Invalid, pattern);
        }

        [Test]
        public void IsSequence_WithJoker_ReturnsFalse()
        {
            // Arrange: Phase 1.5 does not support Joker in sequences
            var cards = new List<CardSO>
            {
                TestHelpers.CreateCard(CardSO.Suit.Spade, 3),
                TestHelpers.CreateJoker(isRed: true),
                TestHelpers.CreateCard(CardSO.Suit.Spade, 5)
            };

            // Act
            bool isSequence = detector.IsSequence(cards);

            // Assert
            Assert.IsFalse(isSequence);
        }

        // ========== Joker in Pairs/Triples/Quadruples (Expected to work) ==========

        [Test]
        public void DetectPattern_JokerPair_ReturnsPair()
        {
            // Arrange: Joker + Joker should form a pair
            var cards = new List<CardSO>
            {
                TestHelpers.CreateJoker(isRed: true),
                TestHelpers.CreateJoker(isRed: false)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Pair, pattern, "Joker + Joker should form a pair");
        }

        [Test]
        public void DetectPattern_JokerAsWildcardPair_ReturnsPair()
        {
            // Arrange: Joker + normal card should form a pair (Joker as wildcard)
            var cards = new List<CardSO>
            {
                TestHelpers.CreateJoker(isRed: true),
                TestHelpers.CreateCard(CardSO.Suit.Heart, 5)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Pair, pattern, "Joker + 5 should form a pair (wildcard)");
        }

        [Test]
        public void DetectPattern_JokerAsWildcardTriple_ReturnsTriple()
        {
            // Arrange: Joker + two normal cards should form a triple (Joker as wildcard)
            var cards = new List<CardSO>
            {
                TestHelpers.CreateJoker(isRed: true),
                TestHelpers.CreateCard(CardSO.Suit.Heart, 5),
                TestHelpers.CreateCard(CardSO.Suit.Diamond, 5)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Triple, pattern, "Joker + 5 + 5 should form a triple (wildcard)");
        }

        [Test]
        public void DetectPattern_TwoJokersAsWildcardQuadruple_ReturnsQuadruple()
        {
            // Arrange: 2 Jokers + two normal cards should form a quadruple (Jokers as wildcard)
            var cards = new List<CardSO>
            {
                TestHelpers.CreateJoker(isRed: true),
                TestHelpers.CreateJoker(isRed: false),
                TestHelpers.CreateCard(CardSO.Suit.Heart, 5),
                TestHelpers.CreateCard(CardSO.Suit.Diamond, 5)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Quadruple, pattern, "2 Jokers + 5 + 5 should form a quadruple (wildcard)");
        }

        [Test]
        public void DetectPattern_JokerWithDifferentRanks_ReturnsInvalid()
        {
            // Arrange: Joker + two different rank cards cannot form valid pattern
            var cards = new List<CardSO>
            {
                TestHelpers.CreateJoker(isRed: true),
                TestHelpers.CreateCard(CardSO.Suit.Heart, 5),
                TestHelpers.CreateCard(CardSO.Suit.Diamond, 7)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Invalid, pattern, "Joker + 5 + 7 should be invalid (different ranks)");
        }

        [Test]
        public void DetectPattern_AllJokersTriple_ReturnsTriple()
        {
            // Arrange: 3 Jokers should form a triple (edge case: all wildcards)
            // Note: In actual game, only 2 Jokers exist, but test the logic
            var cards = new List<CardSO>
            {
                TestHelpers.CreateJoker(isRed: true),
                TestHelpers.CreateJoker(isRed: false),
                TestHelpers.CreateJoker(isRed: true)  // Simulating 3rd Joker for testing
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Triple, pattern, "3 Jokers should form a triple");
        }

        [Test]
        public void DetectPattern_JokerWithSingleNormalCard_AndTwoMoreDifferent_ReturnsInvalid()
        {
            // Arrange: Joker + 5 + 7 + 7 (Joker could match 5 or 7, but not both)
            var cards = new List<CardSO>
            {
                TestHelpers.CreateJoker(isRed: true),
                TestHelpers.CreateCard(CardSO.Suit.Heart, 5),
                TestHelpers.CreateCard(CardSO.Suit.Diamond, 7),
                TestHelpers.CreateCard(CardSO.Suit.Club, 7)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Invalid, pattern, "Joker + 5 + 7 + 7 should be invalid (mixed ranks)");
        }

        [Test]
        public void DetectPattern_ThreeJokersWithOneNormalCard_ReturnsQuadruple()
        {
            // Arrange: 3 Jokers + 5 should form a quadruple (all Jokers match the 5)
            var cards = new List<CardSO>
            {
                TestHelpers.CreateJoker(isRed: true),
                TestHelpers.CreateJoker(isRed: false),
                TestHelpers.CreateJoker(isRed: true),  // 3rd Joker
                TestHelpers.CreateCard(CardSO.Suit.Heart, 5)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Quadruple, pattern, "3 Jokers + 5 should form a quadruple");
        }

        [Test]
        public void DetectPattern_JokerWith2_ReturnsPair()
        {
            // Arrange: Joker + 2 (strongest normal card) should form a pair
            var cards = new List<CardSO>
            {
                TestHelpers.CreateJoker(isRed: true),
                TestHelpers.CreateCard(CardSO.Suit.Heart, 2)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Pair, pattern, "Joker + 2 should form a pair");
        }

        [Test]
        public void DetectPattern_JokerWithAce_ReturnsPair()
        {
            // Arrange: Joker + Ace should form a pair
            var cards = new List<CardSO>
            {
                TestHelpers.CreateJoker(isRed: true),
                TestHelpers.CreateCard(CardSO.Suit.Spade, 1)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Pair, pattern, "Joker + A should form a pair");
        }

        [Test]
        public void DetectPattern_JokerWith8_ReturnsPair()
        {
            // Arrange: Joker + 8 (8-cut card) should form a pair
            var cards = new List<CardSO>
            {
                TestHelpers.CreateJoker(isRed: true),
                TestHelpers.CreateCard(CardSO.Suit.Diamond, 8)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Pair, pattern, "Joker + 8 should form a pair");
        }

        [Test]
        public void DetectPattern_JokerWithSpade3_ReturnsPair()
        {
            // Arrange: Joker + Spade 3 (special card) should form a pair
            var cards = new List<CardSO>
            {
                TestHelpers.CreateJoker(isRed: true),
                TestHelpers.CreateCard(CardSO.Suit.Spade, 3)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Pair, pattern, "Joker + Spade 3 should form a pair");
        }

        [Test]
        public void DetectPattern_TwoJokersWithTwoNormalCards_DifferentRanks_ReturnsInvalid()
        {
            // Arrange: 2 Jokers + 5 + 7 (different normal card ranks)
            var cards = new List<CardSO>
            {
                TestHelpers.CreateJoker(isRed: true),
                TestHelpers.CreateJoker(isRed: false),
                TestHelpers.CreateCard(CardSO.Suit.Heart, 5),
                TestHelpers.CreateCard(CardSO.Suit.Diamond, 7)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Invalid, pattern, "2 Jokers + 5 + 7 should be invalid (different ranks)");
        }

        [Test]
        public void DetectPattern_OneJokerWithThreeNormalCards_SameRank_ReturnsQuadruple()
        {
            // Arrange: Joker + 5 + 5 + 5 should form a quadruple
            var cards = new List<CardSO>
            {
                TestHelpers.CreateJoker(isRed: true),
                TestHelpers.CreateCard(CardSO.Suit.Heart, 5),
                TestHelpers.CreateCard(CardSO.Suit.Diamond, 5),
                TestHelpers.CreateCard(CardSO.Suit.Club, 5)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Quadruple, pattern, "Joker + 5 + 5 + 5 should form a quadruple");
        }

        [Test]
        public void DetectPattern_TwoJokersWithTwoNormalCards_SameRank_ReturnsQuadruple()
        {
            // Arrange: 2 Jokers + 5 + 5 should form a quadruple (already added above, confirming)
            var cards = new List<CardSO>
            {
                TestHelpers.CreateJoker(isRed: true),
                TestHelpers.CreateJoker(isRed: false),
                TestHelpers.CreateCard(CardSO.Suit.Heart, 5),
                TestHelpers.CreateCard(CardSO.Suit.Diamond, 5)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Quadruple, pattern, "2 Jokers + 5 + 5 should form a quadruple (revolution trigger)");
        }

        // ========== Invalid Patterns ==========

        [Test]
        public void DetectPattern_NullCards_ReturnsInvalid()
        {
            // Arrange
            List<CardSO> cards = null;

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Invalid, pattern);
        }

        [Test]
        public void DetectPattern_EmptyList_ReturnsInvalid()
        {
            // Arrange
            var cards = new List<CardSO>();

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Invalid, pattern);
        }

        [Test]
        public void DetectPattern_DifferentRanksNotSequence_ReturnsInvalid()
        {
            // Arrange: 2 cards of different ranks (not a pair, not a sequence)
            var cards = new List<CardSO>
            {
                TestHelpers.CreateCard(CardSO.Suit.Spade, 5),
                TestHelpers.CreateCard(CardSO.Suit.Spade, 7)
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Invalid, pattern);
        }

        [Test]
        public void DetectPattern_5OfAKind_ReturnsInvalid()
        {
            // Arrange: Phase 1.5 does not support 5+ of a kind
            var cards = new List<CardSO>
            {
                TestHelpers.CreateCard(CardSO.Suit.Spade, 5),
                TestHelpers.CreateCard(CardSO.Suit.Heart, 5),
                TestHelpers.CreateCard(CardSO.Suit.Diamond, 5),
                TestHelpers.CreateCard(CardSO.Suit.Club, 5),
                TestHelpers.CreateCard(CardSO.Suit.Spade, 5)  // Duplicate for testing
            };

            // Act
            PlayPattern pattern = detector.DetectPattern(cards);

            // Assert
            Assert.AreEqual(PlayPattern.Invalid, pattern);
        }

        // ========== Sequence Strength Calculation ==========

        [Test]
        public void GetSequenceStrength_Normal_ReturnsMaxStrength()
        {
            // Arrange
            var cards = new List<CardSO>
            {
                TestHelpers.CreateCard(CardSO.Suit.Spade, 3),  // Strength: 3
                TestHelpers.CreateCard(CardSO.Suit.Spade, 4),  // Strength: 4
                TestHelpers.CreateCard(CardSO.Suit.Spade, 5)   // Strength: 5
            };

            // Act
            int strength = detector.GetSequenceStrength(cards, isRevolution: false);

            // Assert
            Assert.AreEqual(5, strength);  // Max strength
        }

        [Test]
        public void GetSequenceStrength_Revolution_ReturnsMinStrength()
        {
            // Arrange
            var cards = new List<CardSO>
            {
                TestHelpers.CreateCard(CardSO.Suit.Spade, 3),  // Revolution strength: 15
                TestHelpers.CreateCard(CardSO.Suit.Spade, 4),  // Revolution strength: 14
                TestHelpers.CreateCard(CardSO.Suit.Spade, 5)   // Revolution strength: 13
            };

            // Act
            int strength = detector.GetSequenceStrength(cards, isRevolution: true);

            // Assert
            Assert.AreEqual(13, strength);  // Min strength (3→15, 4→14, 5→13, min=13)
        }

        [Test]
        public void GetSequenceStrength_NormalHighCards_ReturnsMaxStrength()
        {
            // Arrange: J-Q-K sequence
            var cards = new List<CardSO>
            {
                TestHelpers.CreateCard(CardSO.Suit.Heart, 11),  // J, Strength: 11
                TestHelpers.CreateCard(CardSO.Suit.Heart, 12),  // Q, Strength: 12
                TestHelpers.CreateCard(CardSO.Suit.Heart, 13)   // K, Strength: 13
            };

            // Act
            int strength = detector.GetSequenceStrength(cards, isRevolution: false);

            // Assert
            Assert.AreEqual(13, strength);  // Max strength (K)
        }

        [Test]
        public void GetSequenceStrength_EmptyList_ReturnsZero()
        {
            // Arrange
            var cards = new List<CardSO>();

            // Act
            int strength = detector.GetSequenceStrength(cards, isRevolution: false);

            // Assert
            Assert.AreEqual(0, strength);
        }
    }
}

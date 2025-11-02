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

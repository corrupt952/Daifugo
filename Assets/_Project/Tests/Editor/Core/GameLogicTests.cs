using NUnit.Framework;
using Daifugo.Core;
using Daifugo.Data;
using Daifugo.Tests.Helpers;

namespace Daifugo.Tests.Core
{
    /// <summary>
    /// Tests for GameLogic class
    /// Validates card play logic, special rules (8-cut), and win conditions
    /// </summary>
    public class GameLogicTests
    {
        private GameLogic gameLogic;
        private GameRulesSO gameRules;

        /// <summary>
        /// Sets up test fixtures before each test
        /// Creates fresh instances to ensure test isolation
        /// </summary>
        [SetUp]
        public void Setup()
        {
            gameRules = TestHelpers.CreateGameRules(enable8Cut: true);
            gameLogic = new GameLogic(gameRules);
        }

        #region Happy Path Tests

        /// <summary>
        /// Test: Playing a valid card on empty field should succeed
        /// </summary>
        [Test]
        public void PlayCard_OnEmptyField_ReturnsSuccess()
        {
            // Arrange
            CardSO card = TestHelpers.CreateCardByRank(5);
            CardSO card2 = TestHelpers.CreateCardByRank(7);
            PlayerHandSO hand = TestHelpers.CreateHand(0, card, card2);
            FieldState fieldState = FieldState.Empty();

            // Act
            CardPlayResult result = gameLogic.PlayCard(card, hand, fieldState);

            // Assert
            Assert.IsTrue(result.IsSuccess, "Card play should succeed");
            Assert.AreEqual(card, result.NewFieldState.CurrentCard, "Field card should be the played card");
            Assert.IsFalse(result.IsWin, "Should not win with cards remaining");
            Assert.IsFalse(result.ShouldResetField, "Non-8 card should not reset field");
            Assert.AreEqual(TurnAdvanceType.NextPlayer, result.TurnAdvanceType, "Normal card play should advance to next player");
        }

        /// <summary>
        /// Test: Playing a stronger card on existing field should succeed
        /// </summary>
        [Test]
        public void PlayCard_StrongerCard_ReturnsSuccess()
        {
            // Arrange
            CardSO card = TestHelpers.CreateCardByRank(7);
            CardSO card2 = TestHelpers.CreateCardByRank(10);
            FieldState fieldState = FieldState.AddCard(FieldState.Empty(), TestHelpers.CreateCardByRank(5));
            PlayerHandSO hand = TestHelpers.CreateHand(0, card, card2);

            // Act
            CardPlayResult result = gameLogic.PlayCard(card, hand, fieldState);

            // Assert
            Assert.IsTrue(result.IsSuccess, "Playing stronger card should succeed");
            Assert.AreEqual(card, result.NewFieldState.CurrentCard, "Field should update to new card");
            Assert.AreEqual(TurnAdvanceType.NextPlayer, result.TurnAdvanceType, "Normal card play should advance to next player");
        }

        /// <summary>
        /// Test: Playing last card should trigger win condition
        /// </summary>
        [Test]
        public void PlayCard_LastCard_ReturnsWin()
        {
            // Arrange
            CardSO lastCard = TestHelpers.CreateCardByRank(10);
            PlayerHandSO hand = TestHelpers.CreateHand(0, lastCard);

            // Act
            CardPlayResult result = gameLogic.PlayCard(lastCard, hand, FieldState.Empty());

            // Assert
            Assert.IsTrue(result.IsSuccess, "Last card play should succeed");
            Assert.IsTrue(result.IsWin, "Playing last card should trigger win");
            Assert.IsTrue(hand.IsEmpty, "Hand should be empty after playing last card");
            Assert.AreEqual(TurnAdvanceType.GameEnd, result.TurnAdvanceType, "Winning should end game");
        }

        /// <summary>
        /// Test: Playing card from multi-card hand should not trigger win
        /// </summary>
        [Test]
        public void PlayCard_WithRemainingCards_DoesNotWin()
        {
            // Arrange
            CardSO card1 = TestHelpers.CreateCardByRank(5);
            CardSO card2 = TestHelpers.CreateCardByRank(7);
            PlayerHandSO hand = TestHelpers.CreateHand(0, card1, card2);

            // Act
            CardPlayResult result = gameLogic.PlayCard(card1, hand, FieldState.Empty());

            // Assert
            Assert.IsTrue(result.IsSuccess, "Card play should succeed");
            Assert.IsFalse(result.IsWin, "Should not win with cards remaining");
            Assert.AreEqual(1, hand.CardCount, "Hand should have 1 card remaining");
            Assert.AreEqual(TurnAdvanceType.NextPlayer, result.TurnAdvanceType, "Normal card play should advance to next player");
        }

        #endregion

        #region 8-Cut Special Rule Tests

        /// <summary>
        /// Test: Playing 8 should activate 8-cut rule
        /// </summary>
        [Test]
        public void PlayCard_With8_ActivatesEightCut()
        {
            // Arrange
            CardSO card8 = TestHelpers.CreateCardByRank(8);
            CardSO card2 = TestHelpers.CreateCardByRank(5);
            PlayerHandSO hand = TestHelpers.CreateHand(0, card8, card2);

            // Act
            CardPlayResult result = gameLogic.PlayCard(card8, hand, FieldState.Empty());

            // Assert
            Assert.IsTrue(result.IsSuccess, "8 card play should succeed");
            Assert.IsTrue(result.ShouldResetField, "8 should activate 8-cut rule");
            Assert.AreEqual(card8, result.NewFieldState.CurrentCard, "Field should update to 8");
            Assert.AreEqual(TurnAdvanceType.SamePlayer, result.TurnAdvanceType, "8-cut should keep same player");
        }

        /// <summary>
        /// Test: Playing 8 on existing field should still activate 8-cut
        /// </summary>
        [Test]
        public void PlayCard_With8OnExistingField_ActivatesEightCut()
        {
            // Arrange
            CardSO card8 = TestHelpers.CreateCardByRank(8);
            CardSO card2 = TestHelpers.CreateCardByRank(10);
            FieldState fieldState = FieldState.AddCard(FieldState.Empty(), TestHelpers.CreateCardByRank(5));
            PlayerHandSO hand = TestHelpers.CreateHand(0, card8, card2);

            // Act
            CardPlayResult result = gameLogic.PlayCard(card8, hand, fieldState);

            // Assert
            Assert.IsTrue(result.IsSuccess, "8 play should succeed");
            Assert.IsTrue(result.ShouldResetField, "8 should reset field regardless of existing card");
            Assert.AreEqual(TurnAdvanceType.SamePlayer, result.TurnAdvanceType, "8-cut should keep same player");
        }

        /// <summary>
        /// Test: Playing last card 8 should trigger both win and 8-cut
        /// </summary>
        [Test]
        public void PlayCard_LastCard8_TriggersWinAndEightCut()
        {
            // Arrange
            CardSO card8 = TestHelpers.CreateCardByRank(8);
            PlayerHandSO hand = TestHelpers.CreateHand(0, card8);

            // Act
            CardPlayResult result = gameLogic.PlayCard(card8, hand, FieldState.Empty());

            // Assert
            Assert.IsTrue(result.IsSuccess, "Last 8 play should succeed");
            Assert.IsTrue(result.IsWin, "Should win with last card");
            Assert.IsTrue(result.ShouldResetField, "8 should activate 8-cut even when winning");
            Assert.AreEqual(TurnAdvanceType.GameEnd, result.TurnAdvanceType, "Win takes priority - game should end");
        }

        /// <summary>
        /// Test: Non-8 cards should not activate 8-cut
        /// </summary>
        [Test]
        public void PlayCard_Non8Card_DoesNotActivateEightCut()
        {
            // Arrange - Test all non-8 ranks
            int[] nonEightRanks = { 1, 2, 3, 4, 5, 6, 7, 9, 10, 11, 12, 13 };

            foreach (int rank in nonEightRanks)
            {
                CardSO card = TestHelpers.CreateCardByRank(rank);
                CardSO card2 = TestHelpers.CreateCardByRank(4);
                PlayerHandSO hand = TestHelpers.CreateHand(0, card, card2);

                // Act
                CardPlayResult result = gameLogic.PlayCard(card, hand, FieldState.Empty());

                // Assert
                Assert.IsTrue(result.IsSuccess, $"Card {rank} play should succeed");
                Assert.IsFalse(result.ShouldResetField, $"Card {rank} should not activate 8-cut");
                Assert.AreEqual(TurnAdvanceType.NextPlayer, result.TurnAdvanceType, $"Card {rank} should advance to next player");
            }
        }

        /// <summary>
        /// Test: After 8-cut, any card should be playable on empty field
        /// </summary>
        [Test]
        public void PlayCard_After8Cut_AnyCardPlayable()
        {
            // Arrange
            CardSO card8 = TestHelpers.CreateCardByRank(8);
            CardSO weakCard = TestHelpers.CreateCardByRank(3);
            CardSO card3 = TestHelpers.CreateCardByRank(6);
            PlayerHandSO hand = TestHelpers.CreateHand(0, card8, weakCard, card3);

            // Act - Play 8 first
            CardPlayResult result1 = gameLogic.PlayCard(card8, hand, FieldState.Empty());

            // Assert - 8-cut should be triggered
            Assert.IsTrue(result1.ShouldResetField, "8 should trigger field reset");

            // Act - Play weak card on empty field (simulating field reset)
            CardPlayResult result2 = gameLogic.PlayCard(weakCard, hand, FieldState.Empty());

            // Assert - Any card should be playable on empty field
            Assert.IsTrue(result2.IsSuccess, "Any card should be playable after 8-cut (empty field)");
            Assert.AreEqual(TurnAdvanceType.NextPlayer, result2.TurnAdvanceType, "Normal card after 8-cut should advance to next player");
        }

        #endregion

        #region 11-Back (Temporary Revolution) Special Rule Tests

        /// <summary>
        /// Test: Playing J should activate 11-back rule
        /// </summary>
        [Test]
        public void PlayCard_WithJ_Activates11Back()
        {
            // Arrange
            gameRules = TestHelpers.CreateGameRules(enable11Back: true);
            gameLogic = new GameLogic(gameRules);

            CardSO cardJ = TestHelpers.CreateCardByRank(11); // J
            CardSO card2 = TestHelpers.CreateCardByRank(5);
            PlayerHandSO hand = TestHelpers.CreateHand(0, cardJ, card2);

            // Act
            CardPlayResult result = gameLogic.PlayCard(cardJ, hand, FieldState.Empty());

            // Assert
            Assert.IsTrue(result.IsSuccess, "J card play should succeed");
            Assert.IsTrue(result.ShouldActivate11Back, "J should activate 11-back rule");
            Assert.IsTrue(result.NewFieldState.IsTemporaryRevolution, "Field should have temporary revolution");
            Assert.AreEqual(TurnAdvanceType.NextPlayer, result.TurnAdvanceType, "11-back should advance to next player (field not reset)");
        }

        /// <summary>
        /// Test: Playing J when 11-back is disabled should not activate rule
        /// </summary>
        [Test]
        public void PlayCard_WithJ_11BackDisabled_DoesNotActivate()
        {
            // Arrange
            gameRules = TestHelpers.CreateGameRules(enable11Back: false);
            gameLogic = new GameLogic(gameRules);

            CardSO cardJ = TestHelpers.CreateCardByRank(11);
            CardSO card2 = TestHelpers.CreateCardByRank(5);
            PlayerHandSO hand = TestHelpers.CreateHand(0, cardJ, card2);

            // Act
            CardPlayResult result = gameLogic.PlayCard(cardJ, hand, FieldState.Empty());

            // Assert
            Assert.IsTrue(result.IsSuccess, "J play should succeed");
            Assert.IsFalse(result.ShouldActivate11Back, "11-back should not activate when disabled");
            Assert.IsFalse(result.NewFieldState.IsTemporaryRevolution, "No temporary revolution when disabled");
        }

        /// <summary>
        /// Test: Playing non-J card should not activate 11-back
        /// </summary>
        [Test]
        public void PlayCard_NonJ_DoesNotActivate11Back()
        {
            // Arrange
            gameRules = TestHelpers.CreateGameRules(enable11Back: true);
            gameLogic = new GameLogic(gameRules);

            int[] nonJRanks = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 13 };

            foreach (int rank in nonJRanks)
            {
                CardSO card = TestHelpers.CreateCardByRank(rank);
                CardSO card2 = TestHelpers.CreateCardByRank(4);
                PlayerHandSO hand = TestHelpers.CreateHand(0, card, card2);

                // Act
                CardPlayResult result = gameLogic.PlayCard(card, hand, FieldState.Empty());

                // Assert
                Assert.IsTrue(result.IsSuccess, $"Card {rank} play should succeed");
                Assert.IsFalse(result.ShouldActivate11Back, $"Card {rank} should not activate 11-back");
            }
        }

        /// <summary>
        /// Test: Playing J as last card should win and activate 11-back
        /// </summary>
        [Test]
        public void PlayCard_LastCardJ_TriggersWinAnd11Back()
        {
            // Arrange
            gameRules = TestHelpers.CreateGameRules(enable11Back: true);
            gameLogic = new GameLogic(gameRules);

            CardSO cardJ = TestHelpers.CreateCardByRank(11);
            PlayerHandSO hand = TestHelpers.CreateHand(0, cardJ);

            // Act
            CardPlayResult result = gameLogic.PlayCard(cardJ, hand, FieldState.Empty());

            // Assert
            Assert.IsTrue(result.IsSuccess, "Last J play should succeed");
            Assert.IsTrue(result.IsWin, "Should win with last card");
            Assert.IsTrue(result.ShouldActivate11Back, "J should activate 11-back even when winning");
            Assert.AreEqual(TurnAdvanceType.GameEnd, result.TurnAdvanceType, "Win takes priority - game should end");
        }

        /// <summary>
        /// Test: During 11-back, J cannot be played on itself (follows normal strength rules)
        /// </summary>
        [Test]
        public void PlayCard_During11Back_JCannotPlayOnJ()
        {
            // Arrange
            gameRules = TestHelpers.CreateGameRules(enable11Back: true);
            gameLogic = new GameLogic(gameRules);

            CardSO cardJ = TestHelpers.CreateCardByRank(11);
            CardSO card10 = TestHelpers.CreateCardByRank(10);
            PlayerHandSO hand = TestHelpers.CreateHand(0, cardJ, card10);

            // Create field with J (revolution ON, current card = J with strength 11)
            FieldState state = FieldState.AddCard(FieldState.Empty(), cardJ, activates11Back: true);

            // Act - Try to play 10 during revolution (10 < 11 in reversed strength)
            CardPlayResult result = gameLogic.PlayCard(card10, hand, state);

            // Assert
            Assert.IsTrue(result.IsSuccess, "10 should be playable during 11-back (10 < 11 reversed)");
            Assert.IsTrue(result.NewFieldState.IsTemporaryRevolution, "Revolution should continue");
        }

        /// <summary>
        /// Test: Playing J with 8 should activate both rules
        /// </summary>
        [Test]
        public void PlayCard_J8_ActivatesBothRules()
        {
            // Note: This test assumes J and 8 can't be the same card (rank 8 vs rank 11)
            // This test verifies the independence of 8-cut and 11-back rules

            // Arrange
            gameRules = TestHelpers.CreateGameRules(enable8Cut: true, enable11Back: true);
            gameLogic = new GameLogic(gameRules);

            CardSO card8 = TestHelpers.CreateCardByRank(8);
            CardSO cardJ = TestHelpers.CreateCardByRank(11);
            CardSO card3 = TestHelpers.CreateCardByRank(5);
            PlayerHandSO hand = TestHelpers.CreateHand(0, card8, cardJ, card3);

            // Act - Play 8 first
            CardPlayResult result8 = gameLogic.PlayCard(card8, hand, FieldState.Empty());

            // Assert - 8 triggers 8-cut
            Assert.IsTrue(result8.ShouldResetField, "8 should activate 8-cut");
            Assert.IsFalse(result8.ShouldActivate11Back, "8 should not activate 11-back");
            Assert.AreEqual(TurnAdvanceType.SamePlayer, result8.TurnAdvanceType, "8-cut should keep same player");

            // Act - Play J after field reset
            CardPlayResult resultJ = gameLogic.PlayCard(cardJ, hand, FieldState.Empty());

            // Assert - J triggers 11-back
            Assert.IsFalse(resultJ.ShouldResetField, "J should not reset field");
            Assert.IsTrue(resultJ.ShouldActivate11Back, "J should activate 11-back");
            Assert.AreEqual(TurnAdvanceType.NextPlayer, resultJ.TurnAdvanceType, "11-back should advance to next player");
        }

        #endregion

        #region Validation Failure Tests

        /// <summary>
        /// Test: Playing card not in hand should fail
        /// </summary>
        [Test]
        public void PlayCard_CardNotInHand_ReturnsFail()
        {
            // Arrange
            CardSO cardNotInHand = TestHelpers.CreateCardByRank(5);
            CardSO cardInHand = TestHelpers.CreateCardByRank(7);
            PlayerHandSO hand = TestHelpers.CreateHand(0, cardInHand); // Hand contains only card 7

            // Act
            CardPlayResult result = gameLogic.PlayCard(cardNotInHand, hand, FieldState.Empty());

            // Assert
            Assert.IsFalse(result.IsSuccess, "Card not in hand should fail");
            Assert.AreEqual("Card not in hand", result.ErrorMessage, "Error message should indicate card not in hand");
            Assert.IsTrue(result.NewFieldState.IsEmpty, "Field state should be empty on failure");
        }

        /// <summary>
        /// Test: Playing invalid card on field should fail
        /// </summary>
        [Test]
        public void PlayCard_InvalidCardOnField_ReturnsFail()
        {
            // Arrange
            CardSO weakCard = TestHelpers.CreateCardByRank(3);
            FieldState fieldState = FieldState.AddCard(FieldState.Empty(), TestHelpers.CreateCardByRank(10));
            PlayerHandSO hand = TestHelpers.CreateHand(0, weakCard);

            // Act - Try to play weak card (3) on strong field (10)
            CardPlayResult result = gameLogic.PlayCard(weakCard, hand, fieldState);

            // Assert
            Assert.IsFalse(result.IsSuccess, "Invalid card play should fail");
            Assert.AreEqual("Cannot play this card on current field", result.ErrorMessage, "Error message should indicate invalid play");
        }

        /// <summary>
        /// Test: Validation failure should not modify hand
        /// </summary>
        [Test]
        public void PlayCard_ValidationFails_DoesNotModifyHand()
        {
            // Arrange
            CardSO cardNotInHand = TestHelpers.CreateCardByRank(5);
            CardSO cardInHand = TestHelpers.CreateCardByRank(7);
            PlayerHandSO hand = TestHelpers.CreateHand(0, cardInHand);
            int initialCardCount = hand.CardCount;

            // Act - Try to play card not in hand
            CardPlayResult result = gameLogic.PlayCard(cardNotInHand, hand, FieldState.Empty());

            // Assert
            Assert.IsFalse(result.IsSuccess, "Play should fail");
            Assert.AreEqual(initialCardCount, hand.CardCount, "Hand should not be modified on validation failure");
        }

        #endregion

        #region Edge Cases

        /// <summary>
        /// Test: Multiple 8s can be played sequentially
        /// </summary>
        [Test]
        public void PlayCard_Multiple8sSequentially_EachActivatesEightCut()
        {
            // Arrange
            CardSO card8_1 = TestHelpers.CreateCardByRank(8);
            CardSO card8_2 = TestHelpers.CreateCardByRank(8);
            PlayerHandSO hand = TestHelpers.CreateHand(0, card8_1, card8_2);

            // Act - Play first 8
            CardPlayResult result1 = gameLogic.PlayCard(card8_1, hand, FieldState.Empty());

            // Assert - First 8
            Assert.IsTrue(result1.IsSuccess, "First 8 play should succeed");
            Assert.IsTrue(result1.ShouldResetField, "First 8 should activate 8-cut");

            // Act - Play second 8 (assuming field was reset)
            CardPlayResult result2 = gameLogic.PlayCard(card8_2, hand, FieldState.Empty());

            // Assert - Second 8
            Assert.IsTrue(result2.IsSuccess, "Second 8 play should succeed");
            Assert.IsTrue(result2.ShouldResetField, "Second 8 should also activate 8-cut");
            Assert.IsTrue(result2.IsWin, "Playing last card (second 8) should win");
            Assert.AreEqual(TurnAdvanceType.GameEnd, result2.TurnAdvanceType, "Win takes priority - game should end");
        }

        /// <summary>
        /// Test: Playing 8 from single-card hand triggers both win and 8-cut
        /// </summary>
        [Test]
        public void PlayCard_Single8InHand_TriggersWinAndEightCut()
        {
            // Arrange
            CardSO card8 = TestHelpers.CreateCardByRank(8);
            PlayerHandSO hand = TestHelpers.CreateHand(0, card8);

            // Act
            CardPlayResult result = gameLogic.PlayCard(card8, hand, FieldState.Empty());

            // Assert
            Assert.IsTrue(result.IsSuccess, "Single 8 play should succeed");
            Assert.IsTrue(result.IsWin, "Playing last card should win");
            Assert.IsTrue(result.ShouldResetField, "8 should activate 8-cut");
            Assert.IsTrue(hand.IsEmpty, "Hand should be empty");
            Assert.AreEqual(TurnAdvanceType.GameEnd, result.TurnAdvanceType, "Win takes priority - game should end");
        }

        /// <summary>
        /// Test: Hand state is correctly updated after successful play
        /// </summary>
        [Test]
        public void PlayCard_Success_UpdatesHandCorrectly()
        {
            // Arrange
            CardSO card1 = TestHelpers.CreateCardByRank(5);
            CardSO card2 = TestHelpers.CreateCardByRank(7);
            CardSO card3 = TestHelpers.CreateCardByRank(9);
            PlayerHandSO hand = TestHelpers.CreateHand(0, card1, card2, card3);

            // Act
            CardPlayResult result = gameLogic.PlayCard(card2, hand, FieldState.Empty());

            // Assert
            Assert.IsTrue(result.IsSuccess, "Card play should succeed");
            Assert.AreEqual(2, hand.CardCount, "Hand should have 2 cards remaining");
            Assert.IsFalse(hand.HasCard(card2), "Played card should be removed from hand");
            Assert.IsTrue(hand.HasCard(card1), "Other cards should remain in hand");
            Assert.IsTrue(hand.HasCard(card3), "Other cards should remain in hand");
        }

        /// <summary>
        /// Test: Null card parameter returns fail
        /// </summary>
        [Test]
        public void PlayCard_NullCard_ReturnsFail()
        {
            // Arrange
            CardSO nullCard = null;
            CardSO validCard = TestHelpers.CreateCardByRank(5);
            PlayerHandSO hand = TestHelpers.CreateHand(0, validCard);

            // Act
            CardPlayResult result = gameLogic.PlayCard(nullCard, hand, FieldState.Empty());

            // Assert
            Assert.IsFalse(result.IsSuccess, "Null card should fail");
            Assert.IsNotNull(result.ErrorMessage, "Error message should be present");
        }

        /// <summary>
        /// Test: Null hand parameter returns fail
        /// </summary>
        [Test]
        public void PlayCard_NullHand_ReturnsFail()
        {
            // Arrange
            CardSO card = TestHelpers.CreateCardByRank(5);
            PlayerHandSO nullHand = null;

            // Act
            CardPlayResult result = gameLogic.PlayCard(card, nullHand, FieldState.Empty());

            // Assert
            Assert.IsFalse(result.IsSuccess, "Null hand should fail");
            Assert.IsNotNull(result.ErrorMessage, "Error message should be present");
        }

        #endregion

        #region Result Structure Tests

        /// <summary>
        /// Test: Success result contains correct data
        /// </summary>
        [Test]
        public void CardPlayResult_Success_ContainsCorrectData()
        {
            // Arrange
            CardSO card = TestHelpers.CreateCardByRank(5);
            PlayerHandSO hand = TestHelpers.CreateHand(0, card);

            // Act
            CardPlayResult result = gameLogic.PlayCard(card, hand, FieldState.Empty());

            // Assert
            Assert.IsTrue(result.IsSuccess, "Result should indicate success");
            Assert.IsNull(result.ErrorMessage, "Success should have no error message");
            Assert.IsFalse(result.NewFieldState.IsEmpty, "Success should have new field state");
            Assert.AreEqual(card, result.NewFieldState.CurrentCard, "New field card should match played card");
        }

        /// <summary>
        /// Test: Failure result contains error message
        /// </summary>
        [Test]
        public void CardPlayResult_Failure_ContainsErrorMessage()
        {
            // Arrange
            CardSO cardNotInHand = TestHelpers.CreateCardByRank(5);
            CardSO cardInHand = TestHelpers.CreateCardByRank(7);
            PlayerHandSO hand = TestHelpers.CreateHand(0, cardInHand);

            // Act - Try to play card not in hand
            CardPlayResult result = gameLogic.PlayCard(cardNotInHand, hand, FieldState.Empty());

            // Assert
            Assert.IsFalse(result.IsSuccess, "Result should indicate failure");
            Assert.IsNotNull(result.ErrorMessage, "Failure should have error message");
            Assert.IsNotEmpty(result.ErrorMessage, "Error message should not be empty");
            Assert.IsTrue(result.NewFieldState.IsEmpty, "Failure should have empty field state");
        }

        #endregion
    }
}

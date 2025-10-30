using Daifugo.Core;
using Daifugo.Data;

namespace Daifugo.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of IRuleValidator for testing
    /// Allows controlling validation results without Unity dependencies
    /// </summary>
    public class MockRuleValidator : IRuleValidator
    {
        private bool isCardInHandResult = true;
        private bool canPlayCardResult = true;

        /// <summary>
        /// Sets the return value for IsCardInHand
        /// </summary>
        /// <param name="value">Return value</param>
        public void SetCardInHandResult(bool value)
        {
            isCardInHandResult = value;
        }

        /// <summary>
        /// Sets the return value for CanPlayCard
        /// </summary>
        /// <param name="value">Return value</param>
        public void SetCanPlayCardResult(bool value)
        {
            canPlayCardResult = value;
        }

        /// <summary>
        /// Sets both validation results to the same value
        /// </summary>
        /// <param name="value">Return value for all validations</param>
        public void SetAllValid(bool value)
        {
            isCardInHandResult = value;
            canPlayCardResult = value;
        }

        /// <summary>
        /// Mocked IsCardInHand - returns configured result
        /// </summary>
        public bool IsCardInHand(CardSO card, PlayerHandSO hand)
        {
            return isCardInHandResult;
        }

        /// <summary>
        /// Mocked CanPlayCard - returns configured result
        /// </summary>
        public bool CanPlayCard(CardSO card, CardSO currentFieldCard)
        {
            return canPlayCardResult;
        }
    }
}

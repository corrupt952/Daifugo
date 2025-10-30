using Daifugo.Data;
using UnityEngine;

namespace Daifugo.Core
{
    /// <summary>
    /// Validates Daifugo game rules for card plays
    /// Pure C# class (no MonoBehaviour) for testability
    /// Implements IRuleValidator for testability
    /// </summary>
    public class RuleValidator : IRuleValidator
    {
        /// <summary>
        /// Checks if a card can be played given the current field state
        /// </summary>
        /// <param name="card">Card to validate</param>
        /// <param name="currentFieldCard">Card currently on the field (null if empty)</param>
        /// <returns>True if card can be played, false otherwise</returns>
        public bool CanPlayCard(CardSO card, CardSO currentFieldCard)
        {
            if (card == null)
            {
                Debug.LogWarning("[RuleValidator] Cannot validate null card.");
                return false;
            }

            // If field is empty, any card can be played
            if (currentFieldCard == null)
            {
                return true;
            }

            // Card must be stronger than current field card
            int cardStrength = card.GetStrength();
            int fieldStrength = currentFieldCard.GetStrength();

            return cardStrength > fieldStrength;
        }

        /// <summary>
        /// Checks if a player has any playable cards
        /// </summary>
        /// <param name="hand">Player's hand</param>
        /// <param name="currentFieldCard">Card currently on the field (null if empty)</param>
        /// <returns>True if player has at least one playable card, false otherwise</returns>
        public bool HasPlayableCards(PlayerHandSO hand, CardSO currentFieldCard)
        {
            if (hand == null)
            {
                Debug.LogWarning("[RuleValidator] Cannot validate null hand.");
                return false;
            }

            // If field is empty, any card is playable
            if (currentFieldCard == null)
            {
                return hand.CardCount > 0;
            }

            // Check if any card in hand is stronger than field card
            int fieldStrength = currentFieldCard.GetStrength();
            foreach (var card in hand.Cards)
            {
                if (card.GetStrength() > fieldStrength)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the field strength for comparison
        /// </summary>
        /// <param name="currentFieldCard">Card currently on the field (null if empty)</param>
        /// <returns>Field strength (0 if empty)</returns>
        public int GetFieldStrength(CardSO currentFieldCard)
        {
            if (currentFieldCard == null)
            {
                return 0;
            }

            return currentFieldCard.GetStrength();
        }

        /// <summary>
        /// Validates a card belongs to a specific hand
        /// </summary>
        /// <param name="card">Card to check</param>
        /// <param name="hand">Hand to validate against</param>
        /// <returns>True if card is in hand, false otherwise</returns>
        public bool IsCardInHand(CardSO card, PlayerHandSO hand)
        {
            if (card == null || hand == null)
            {
                Debug.LogWarning("[RuleValidator] Cannot validate null card or hand.");
                return false;
            }

            return hand.HasCard(card);
        }
    }
}

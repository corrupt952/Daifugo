using System.Collections.Generic;
using Daifugo.Data;

namespace Daifugo.Core
{
    /// <summary>
    /// Service for determining playable cards based on game rules
    /// Pure logic class (no MonoBehaviour) for testability
    /// </summary>
    public class PlayableCardService
    {
        /// <summary>
        /// Gets playable cards for a specific player during their turn
        /// Returns empty list if not the player's turn
        /// </summary>
        /// <param name="currentPlayerID">The current player ID whose turn it is</param>
        /// <param name="targetPlayerID">The player ID to get playable cards for</param>
        /// <param name="hand">The player's hand</param>
        /// <param name="fieldCard">The current card on the field (null if empty)</param>
        /// <returns>List of playable cards (empty if not player's turn)</returns>
        public List<CardSO> GetPlayableCardsForPlayer(
            int currentPlayerID,
            int targetPlayerID,
            PlayerHandSO hand,
            CardSO fieldCard)
        {
            // Validate inputs
            if (hand == null)
            {
                return new List<CardSO>();
            }

            // Not player's turn - return empty list (no cards are playable)
            if (currentPlayerID != targetPlayerID)
            {
                return new List<CardSO>();
            }

            // Get field strength (0 if no card on field)
            int fieldStrength = fieldCard != null ? fieldCard.GetStrength() : 0;

            // Return playable cards based on field strength
            return hand.GetPlayableCards(fieldStrength);
        }
    }
}

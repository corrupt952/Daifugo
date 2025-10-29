using System.Collections.Generic;
using Tang3cko.EventChannels;
using UnityEngine;

namespace Daifugo.Core
{
    /// <summary>
    /// Manages turn progression and field reset logic using parent tracking method
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        [Header("Events")]
        [Tooltip("Raised when turn changes to a new player")]
        [SerializeField] private IntEventChannelSO onTurnChanged;

        [Tooltip("Raised when field is reset (parent's turn returns after all others passed)")]
        [SerializeField] private VoidEventChannelSO onFieldReset;

        // Runtime state
        private int currentPlayerID = 0;
        private int lastCardPlayerID = -1; // Last player who played a card (parent)
        private HashSet<int> passedPlayers = new(); // Players who passed since last card play
        private const int PLAYER_COUNT = 4;

        /// <summary>
        /// Initializes the turn manager for a new game
        /// </summary>
        public void Initialize()
        {
            currentPlayerID = 0; // Start with player 0 (human) - see C-001
            lastCardPlayerID = -1; // No parent at game start
            passedPlayers.Clear();

            // Notify initial turn
            onTurnChanged.RaiseEvent(currentPlayerID);
        }

        /// <summary>
        /// Advances to the next player's turn
        /// </summary>
        public void NextTurn()
        {
            // Advance to next player (clockwise)
            currentPlayerID = (currentPlayerID + 1) % PLAYER_COUNT;

            // Field reset check: parent's turn returns after all others passed
            if (lastCardPlayerID != -1 && currentPlayerID == lastCardPlayerID)
            {
                // Check if all other players (except parent) have passed
                if (passedPlayers.Count == PLAYER_COUNT - 1)
                {
                    ResetField();
                }
            }

            // Notify turn changed
            onTurnChanged.RaiseEvent(currentPlayerID);
        }

        /// <summary>
        /// Handles a card being played
        /// </summary>
        /// <param name="playerID">Player who played the card</param>
        public void OnCardPlayed(int playerID)
        {
            lastCardPlayerID = playerID; // Update parent
            passedPlayers.Clear(); // Clear pass records
            NextTurn();
        }

        /// <summary>
        /// Handles a player passing their turn
        /// </summary>
        public void OnPlayerPass()
        {
            passedPlayers.Add(currentPlayerID); // Record pass
            NextTurn();
        }

        /// <summary>
        /// Resets the field (clears parent and pass records)
        /// </summary>
        private void ResetField()
        {
            lastCardPlayerID = -1; // Clear parent
            passedPlayers.Clear(); // Clear pass records

            // Notify field reset
            onFieldReset.RaiseEvent();
        }

        /// <summary>
        /// Gets the current player ID
        /// </summary>
        /// <returns>Current player ID (0-3)</returns>
        public int GetCurrentPlayer()
        {
            return currentPlayerID;
        }
    }
}

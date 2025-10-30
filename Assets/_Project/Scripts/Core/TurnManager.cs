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
        /// Handles a card being played (legacy method - use AdvanceTurn instead)
        /// </summary>
        /// <param name="playerID">Player who played the card</param>
        public void OnCardPlayed(int playerID)
        {
            lastCardPlayerID = playerID; // Update parent
            passedPlayers.Clear(); // Clear pass records
            NextTurn();
        }

        /// <summary>
        /// Advances turn based on turn advancement type
        /// </summary>
        /// <param name="playerID">Player who played the card</param>
        /// <param name="advanceType">How to advance the turn</param>
        public void AdvanceTurn(int playerID, TurnAdvanceType advanceType)
        {
            switch (advanceType)
            {
                case TurnAdvanceType.NextPlayer:
                    // Normal card play: update parent, clear passes, advance turn
                    lastCardPlayerID = playerID;
                    passedPlayers.Clear();
                    NextTurn();
                    break;

                case TurnAdvanceType.SamePlayer:
                    // Special rule (8-cut, etc.): update parent, clear passes, stay on same player
                    lastCardPlayerID = playerID;
                    passedPlayers.Clear();
                    onTurnChanged.RaiseEvent(currentPlayerID);
                    break;

                case TurnAdvanceType.SkipFinished:
                    // Future: Skip finished players
                    Debug.LogWarning("[TurnManager] SkipFinished not yet implemented");
                    break;

                case TurnAdvanceType.GameEnd:
                    // Game ended: do nothing (GameManager handles this)
                    break;
            }
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

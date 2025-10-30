using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Daifugo.Data
{
    /// <summary>
    /// Manages a single player's hand of cards
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerHand", menuName = "Daifugo/Data/PlayerHand")]
    public class PlayerHandSO : ScriptableObject
    {
        [Header("Player Info")]
        [Tooltip("Player identifier (0 = Human, 1-3 = AI)")]
        [SerializeField] private int playerID;

        [Header("Runtime State")]
        [Tooltip("Cards currently in hand (modified at runtime)")]
        private List<CardSO> cardsInHand = new();

        /// <summary>
        /// Gets the player ID
        /// </summary>
        public int PlayerID => playerID;

        /// <summary>
        /// Gets the current cards in hand (read-only)
        /// </summary>
        public IReadOnlyList<CardSO> Cards => cardsInHand;

        /// <summary>
        /// Gets the number of cards in hand
        /// </summary>
        public int CardCount => cardsInHand.Count;

        /// <summary>
        /// Checks if the hand is empty
        /// </summary>
        public bool IsEmpty => cardsInHand.Count == 0;

        /// <summary>
        /// Adds a card to the hand
        /// </summary>
        /// <param name="card">Card to add</param>
        public void AddCard(CardSO card)
        {
            if (card == null)
            {
                Debug.LogWarning($"[PlayerHandSO] Player {playerID} - Cannot add null card.", this);
                return;
            }

            cardsInHand.Add(card);
        }

        /// <summary>
        /// Removes a card from the hand
        /// </summary>
        /// <param name="card">Card to remove</param>
        /// <returns>True if card was found and removed, false otherwise</returns>
        public bool RemoveCard(CardSO card)
        {
            if (card == null)
            {
                Debug.LogWarning($"[PlayerHandSO] Player {playerID} - Cannot remove null card.", this);
                return false;
            }

            bool removed = cardsInHand.Remove(card);
            if (!removed)
            {
                Debug.LogWarning($"[PlayerHandSO] Player {playerID} - Card {card.CardSuit} {card.Rank} not found in hand.", this);
            }

            return removed;
        }

        /// <summary>
        /// Checks if the hand contains a specific card
        /// </summary>
        /// <param name="card">Card to check</param>
        /// <returns>True if card is in hand, false otherwise</returns>
        public bool HasCard(CardSO card)
        {
            return card != null && cardsInHand.Contains(card);
        }

        /// <summary>
        /// Sorts the hand by card strength in ascending order (weak to strong)
        /// </summary>
        public void SortByStrength()
        {
            cardsInHand = cardsInHand
                .OrderBy(card => card.GetStrength())
                .ToList();
        }

        /// <summary>
        /// Sorts the hand by rank (ascending)
        /// </summary>
        public void SortByRank()
        {
            cardsInHand = cardsInHand
                .OrderBy(card => card.Rank)
                .ThenBy(card => card.CardSuit)
                .ToList();
        }

        /// <summary>
        /// Gets all cards of a specific rank
        /// </summary>
        /// <param name="rank">Target rank (1-13)</param>
        /// <returns>List of cards with matching rank</returns>
        public List<CardSO> GetCardsByRank(int rank)
        {
            return cardsInHand
                .Where(card => card.Rank == rank)
                .ToList();
        }

        /// <summary>
        /// Clears all cards from the hand
        /// </summary>
        public void Clear()
        {
            cardsInHand.Clear();
        }

        /// <summary>
        /// Initializes the hand for a new game
        /// </summary>
        public void Initialize()
        {
            Clear();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Validate playerID is within valid range
            if (playerID < 0 || playerID > 3)
            {
                Debug.LogWarning($"[PlayerHandSO] Player ID {playerID} is outside valid range (0-3).", this);
            }
        }
#endif
    }
}

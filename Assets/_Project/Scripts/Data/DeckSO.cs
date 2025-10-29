using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Daifugo.Data
{
    /// <summary>
    /// Manages the deck of 52 cards for Daifugo game
    /// </summary>
    [CreateAssetMenu(fileName = "Deck", menuName = "Daifugo/Data/Deck")]
    public class DeckSO : ScriptableObject
    {
        [Header("Deck Configuration")]
        [Tooltip("All 52 CardSO references (13 ranks × 4 suits)")]
        [SerializeField] private List<CardSO> allCards = new();

        [Header("Runtime State")]
        [Tooltip("Current shuffled deck (modified at runtime)")]
        private List<CardSO> currentDeck = new();

        /// <summary>
        /// Gets the original unshuffled card list (read-only)
        /// </summary>
        public IReadOnlyList<CardSO> AllCards => allCards;

        /// <summary>
        /// Gets the current deck for validation (read-only)
        /// </summary>
        public IReadOnlyList<CardSO> CurrentDeck => currentDeck;

        /// <summary>
        /// Gets the current deck count
        /// </summary>
        public int RemainingCards => currentDeck.Count;

        /// <summary>
        /// Initializes and shuffles the deck for a new game
        /// </summary>
        public void Initialize()
        {
            // Validate deck has exactly 52 cards
            if (allCards == null || allCards.Count != 52)
            {
                Debug.LogError($"[DeckSO] Initialize: allCards must contain exactly 52 cards. Current count: {allCards?.Count ?? 0}", this);
                currentDeck.Clear();
                return;
            }

            // Copy all cards to current deck
            currentDeck = new List<CardSO>(allCards);

            // Shuffle the deck
            Shuffle();
        }

        /// <summary>
        /// Shuffles the current deck using Fisher-Yates algorithm
        /// </summary>
        public void Shuffle()
        {
            int n = currentDeck.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (currentDeck[i], currentDeck[j]) = (currentDeck[j], currentDeck[i]);
            }
        }

        /// <summary>
        /// Draws a single card from the top of the deck
        /// </summary>
        /// <returns>The drawn card, or null if deck is empty</returns>
        public CardSO DrawCard()
        {
            if (currentDeck.Count == 0)
            {
                Debug.LogWarning("[DeckSO] Cannot draw card - deck is empty.", this);
                return null;
            }

            CardSO card = currentDeck[0];
            currentDeck.RemoveAt(0);
            return card;
        }

        /// <summary>
        /// Distributes cards to multiple hands in round-robin fashion
        /// </summary>
        /// <param name="hands">Array of PlayerHandSO instances to distribute to</param>
        public void DistributeCards(PlayerHandSO[] hands)
        {
            if (hands == null || hands.Length == 0)
            {
                Debug.LogError("[DeckSO] Cannot distribute cards - hands array is null or empty.", this);
                return;
            }

            // Calculate cards per player
            int cardsPerPlayer = currentDeck.Count / hands.Length;

            if (currentDeck.Count % hands.Length != 0)
            {
                Debug.LogWarning($"[DeckSO] {currentDeck.Count} cards cannot be evenly distributed to {hands.Length} players.", this);
            }

            // Distribute in round-robin order
            int handIndex = 0;
            while (currentDeck.Count > 0)
            {
                CardSO card = DrawCard();
                if (card != null)
                {
                    hands[handIndex].AddCard(card);
                    handIndex = (handIndex + 1) % hands.Length;
                }
            }
        }

        /// <summary>
        /// Resets the deck by clearing current state
        /// </summary>
        public void Reset()
        {
            currentDeck.Clear();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Validate deck has exactly 52 cards
            if (allCards.Count != 52)
            {
                Debug.LogWarning($"[DeckSO] Deck should contain 52 cards, but has {allCards.Count} cards.", this);
            }

            // Validate no null references
            if (allCards.Any(card => card == null))
            {
                Debug.LogWarning($"[DeckSO] Deck contains null CardSO references.", this);
            }

            // Check for duplicates (same suit + rank)
            var duplicates = allCards
                .Where(c => c != null)
                .GroupBy(c => (c.CardSuit, c.Rank))
                .Where(g => g.Count() > 1)
                .Select(g => $"{g.Key.CardSuit} {g.Key.Rank}")
                .ToList();

            if (duplicates.Count > 0)
            {
                Debug.LogWarning($"[DeckSO] Duplicate cards detected: {string.Join(", ", duplicates)}", this);
            }
        }
#endif
    }
}

using UnityEngine;

namespace Daifugo.Data
{
    /// <summary>
    /// Immutable card data for Daifugo game
    /// </summary>
    [CreateAssetMenu(fileName = "Card", menuName = "Daifugo/Data/Card")]
    public class CardSO : ScriptableObject
    {
        [Header("Card Properties")]
        [Tooltip("Card suit (Spade, Heart, Diamond, Club)")]
        [SerializeField] private Suit suit;

        [Tooltip("Card rank (1=Ace, 2-10=Numbers, 11=Jack, 12=Queen, 13=King)")]
        [SerializeField] private int rank;

        [Header("Visual")]
        [Tooltip("Card sprite image (Kenney asset)")]
        [SerializeField] private Sprite cardSprite;

        /// <summary>
        /// Gets the card suit
        /// </summary>
        public Suit CardSuit => suit;

        /// <summary>
        /// Gets the card rank (1-13)
        /// </summary>
        public int Rank => rank;

        /// <summary>
        /// Gets the card sprite for display
        /// </summary>
        public Sprite CardSprite => cardSprite;

        /// <summary>
        /// Gets the card strength according to Daifugo rules
        /// </summary>
        /// <returns>Card strength (2=15, Ace=14, King=13, ..., 3=3)</returns>
        public int GetStrength()
        {
            // 2 is strongest
            if (rank == 2) return 15;

            // Ace is second strongest
            if (rank == 1) return 14;

            // Other cards: rank = strength
            return rank;
        }

        /// <summary>
        /// Card suit enumeration
        /// </summary>
        public enum Suit
        {
            Spade,
            Heart,
            Diamond,
            Club
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Validate rank is within valid range
            if (rank < 1 || rank > 13)
            {
                Debug.LogWarning($"[CardSO] Invalid rank {rank} on {name}. Rank must be 1-13.", this);
            }

            // Validate sprite is assigned
            if (cardSprite == null)
            {
                Debug.LogWarning($"[CardSO] cardSprite is not assigned on {name}.", this);
            }
        }
#endif
    }
}

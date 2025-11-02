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
        [Tooltip("Is this card a Joker? (Joker beats all cards)")]
        [SerializeField] private bool isJoker = false;

        [Tooltip("Card suit (Spade, Heart, Diamond, Club) - Ignored for Jokers")]
        [SerializeField] private Suit suit;

        [Tooltip("Card rank (0=Joker, 1=Ace, 2-10=Numbers, 11=Jack, 12=Queen, 13=King)")]
        [SerializeField] private int rank;

        [Header("Visual")]
        [Tooltip("Card sprite image (Kenney asset)")]
        [SerializeField] private Sprite cardSprite;

        /// <summary>
        /// Gets whether this card is a Joker
        /// </summary>
        public bool IsJoker => isJoker;

        /// <summary>
        /// Gets the card suit (meaningless for Jokers)
        /// </summary>
        public Suit CardSuit => suit;

        /// <summary>
        /// Gets the card rank (0=Joker, 1-13=Normal cards)
        /// </summary>
        public int Rank => rank;

        /// <summary>
        /// Gets the card sprite for display
        /// </summary>
        public Sprite CardSprite => cardSprite;

        /// <summary>
        /// Gets the card strength according to Daifugo rules
        /// Phase 1.5: Supports revolution mode where card strengths are reversed
        /// </summary>
        /// <param name="isRevolution">Whether revolution is active (default: false)</param>
        /// <returns>Card strength value for comparison</returns>
        public int GetStrength(bool isRevolution = false)
        {
            // Joker is always strongest (unchanged in revolution)
            if (IsJoker) return 16;

            if (isRevolution)
            {
                // Revolution: weak cards become strong
                if (Rank == 2) return 2;      // 2 is weakest (except Joker)
                if (Rank == 1) return 3;      // A is 3
                return 18 - Rank;             // 3 → 15, 4 → 14, ..., K(13) → 5
            }
            else
            {
                // Normal: Phase 1 logic unchanged
                if (Rank == 2) return 15;     // 2 is strongest (except Joker)
                if (Rank == 1) return 14;     // A is 14
                return Rank;                   // 3-13 unchanged
            }
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
            // Joker validation
            if (isJoker)
            {
                if (rank != 0)
                {
                    Debug.LogWarning($"[CardSO] Joker {name} should have rank 0, but has rank {rank}.", this);
                }
            }
            else
            {
                // Normal card validation
                if (rank < 1 || rank > 13)
                {
                    Debug.LogWarning($"[CardSO] Invalid rank {rank} on {name}. Rank must be 1-13 for normal cards.", this);
                }
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

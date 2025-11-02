namespace Daifugo.Core
{
    /// <summary>
    /// Play pattern types for card combinations
    /// Phase 1.5: Supports single, pairs, triples, quadruples, and sequences
    /// </summary>
    public enum PlayPattern
    {
        /// <summary>Single card (1 card)</summary>
        Single,

        /// <summary>Pair (2 cards of same rank)</summary>
        Pair,

        /// <summary>Triple (3 cards of same rank)</summary>
        Triple,

        /// <summary>Quadruple (4 cards of same rank, triggers revolution)</summary>
        Quadruple,

        /// <summary>Sequence (3+ cards in consecutive rank, same suit)</summary>
        Sequence,

        /// <summary>Invalid combination</summary>
        Invalid
    }
}

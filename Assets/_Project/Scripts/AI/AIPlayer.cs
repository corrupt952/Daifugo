using System.Linq;
using Daifugo.Core;
using Daifugo.Data;
using UnityEngine;

namespace Daifugo.AI
{
    /// <summary>
    /// AI decision logic for computer players
    /// Implements simple greedy strategy: play weakest valid card
    /// </summary>
    public class AIPlayer : MonoBehaviour
    {
        [Header("Components")]
        [Tooltip("Rule validator for checking playable cards")]
        [SerializeField] private RuleValidator ruleValidator;

        /// <summary>
        /// Decides which card to play or whether to pass
        /// </summary>
        /// <param name="hand">AI player's hand</param>
        /// <param name="fieldCard">Current card on the field (null if empty)</param>
        /// <returns>Card to play, or null to pass</returns>
        public CardSO DecideAction(PlayerHandSO hand, CardSO fieldCard)
        {
            // 1. Get playable cards based on field strength
            int fieldStrength = fieldCard?.GetStrength() ?? 0;
            var playableCards = hand.GetPlayableCards(fieldStrength);

            // 2. If no playable cards, return null (pass)
            if (playableCards.Count == 0)
            {
                return null;
            }

            // 3. Select weakest playable card (greedy strategy)
            return playableCards
                .OrderBy(c => c.GetStrength())
                .First();
        }
    }
}

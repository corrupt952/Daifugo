using System.Linq;
using Daifugo.Core;
using Daifugo.Data;

namespace Daifugo.AI
{
    /// <summary>
    /// AI decision logic for computer players
    /// Pure C# class (no MonoBehaviour) for testability
    /// Implements simple greedy strategy: play weakest valid card
    /// </summary>
    public class AIPlayerStrategy
    {
        private readonly PlayableCardsCalculator calculator;
        private readonly GameRulesSO gameRules;

        /// <summary>
        /// Creates a new AIPlayerStrategy instance
        /// </summary>
        /// <param name="gameRules">Game rules configuration</param>
        public AIPlayerStrategy(GameRulesSO gameRules)
        {
            calculator = new PlayableCardsCalculator();
            this.gameRules = gameRules;
        }

        /// <summary>
        /// Decides which card to play or whether to pass
        /// </summary>
        /// <param name="hand">AI player's hand</param>
        /// <param name="fieldState">Current field state (includes card history)</param>
        /// <returns>Card to play, or null to pass</returns>
        public CardSO DecideAction(PlayerHandSO hand, FieldState fieldState)
        {
            // Validate inputs
            if (hand == null)
            {
                return null;
            }

            // 1. Get playable cards based on field state (includes binding check)
            var playableCards = calculator.GetPlayableCards(hand, fieldState, gameRules);

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

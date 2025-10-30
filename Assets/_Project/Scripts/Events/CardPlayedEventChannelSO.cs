using Tang3cko.EventChannels;
using UnityEngine;

namespace Daifugo.Events
{
    /// <summary>
    /// Data structure for card played events
    /// Contains information about the played card, player, and current field state
    /// </summary>
    [System.Serializable]
    public struct CardPlayedEventData
    {
        /// <summary>
        /// The card that was played
        /// </summary>
        public Data.CardSO Card;

        /// <summary>
        /// ID of the player who played the card (0-3)
        /// </summary>
        public int PlayerID;

        /// <summary>
        /// The field state after this play (includes card history for binding detection)
        /// </summary>
        public Core.FieldState FieldState;

        /// <summary>
        /// Callback invoked by UI after card animation completes
        /// Used to ensure special rules and turn advance occur after visual feedback
        /// </summary>
        /// <remarks>
        /// Responsibility:
        /// - GameManager creates this callback containing post-animation logic (special rules, win check, turn advance)
        /// - UI invokes this callback after animation and visual feedback complete
        /// - Ensures proper game progression timeline without GameManager holding state
        ///
        /// Execution timing:
        /// - Called after card animation reaches field
        /// - Called after 0.5s delay to let player visually confirm the played card
        /// - Must use null-conditional operator (?.) when invoking
        ///
        /// Example flow:
        /// 1. GameManager: Creates callback with game logic
        /// 2. GameManager: Raises CardPlayed event with callback
        /// 3. UI: Animates card to field
        /// 4. UI: Waits 0.5s for visual feedback
        /// 5. UI: Invokes callback (triggers special rules, turn advance)
        /// </remarks>
        public System.Action OnAnimationComplete;
    }

    /// <summary>
    /// EventChannel for broadcasting card played events with full context
    /// </summary>
    [CreateAssetMenu(fileName = "CardPlayedEventChannel", menuName = "Daifugo/Events/CardPlayedEventChannel")]
    public class CardPlayedEventChannelSO : EventChannelSO<CardPlayedEventData>
    {
        // EventChannelSO<T> provides:
        // - RaiseEvent(CardPlayedEventData value)
        // - OnEventRaised event (Action<CardPlayedEventData>)
        // No additional implementation needed for basic functionality
    }
}

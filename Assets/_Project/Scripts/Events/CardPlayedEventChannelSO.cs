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
        /// The current card on the field after this play
        /// (Usually the same as Card)
        /// </summary>
        public Data.CardSO FieldCard;
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

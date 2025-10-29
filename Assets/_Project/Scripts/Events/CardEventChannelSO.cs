using Tang3cko.EventChannels;
using UnityEngine;

namespace Daifugo.Events
{
    /// <summary>
    /// EventChannel for broadcasting CardSO events
    /// </summary>
    [CreateAssetMenu(fileName = "CardEventChannel", menuName = "Daifugo/Events/CardEventChannel")]
    public class CardEventChannelSO : EventChannelSO<Data.CardSO>
    {
        // EventChannelSO<T> provides:
        // - RaiseEvent(CardSO value)
        // - OnEventRaised event (Action<CardSO>)
        // No additional implementation needed for basic functionality
    }
}

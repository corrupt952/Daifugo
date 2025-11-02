using System.Collections.Generic;
using Daifugo.Data;
using Tang3cko.EventChannels;
using UnityEngine;

namespace Daifugo.Events
{
    /// <summary>
    /// Event channel for raising events with a list of cards
    /// Phase 1.5: Used for multiple card play events (pairs, triples, sequences)
    /// </summary>
    [CreateAssetMenu(fileName = "ListCardEventChannel", menuName = "Daifugo/Events/ListCardEventChannel")]
    public class ListCardEventChannelSO : EventChannelSO<List<CardSO>>
    {
    }
}

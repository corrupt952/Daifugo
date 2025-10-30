using System.Collections.Generic;
using System.Linq;
using Daifugo.Data;

namespace Daifugo.Core
{
    /// <summary>
    /// 出せる手札を算出する純粋ロジッククラス
    /// Phase 1: 単一カード強度比較のみ
    /// Phase 2以降: 複数枚出し、階段、特殊ルール対応予定
    /// </summary>
    public class PlayableCardsCalculator
    {
        /// <summary>
        /// 手札から出せるカードを取得
        /// </summary>
        /// <param name="hand">現在の手札</param>
        /// <param name="fieldState">場の状態</param>
        /// <param name="gameRules">ゲームルール設定</param>
        /// <returns>出せるカードのリスト</returns>
        public List<CardSO> GetPlayableCards(
            PlayerHandSO hand,
            FieldState fieldState,
            GameRulesSO gameRules)
        {
            if (hand == null) return new List<CardSO>();

            // 場が空 → 全カード出せる
            if (fieldState.IsEmpty)
                return new List<CardSO>(hand.Cards);

            // Phase 1: 単一カード強度比較のみ
            // Phase 2以降: gameRulesを使って革命、縛りなどを判定
            return hand.Cards
                .Where(card => CanPlayCard(card, fieldState, gameRules))
                .ToList();
        }

        /// <summary>
        /// 単一カードが出せるか判定
        /// </summary>
        /// <param name="card">判定するカード</param>
        /// <param name="fieldState">場の状態</param>
        /// <param name="gameRules">ゲームルール設定</param>
        /// <returns>True if card can be played, false otherwise</returns>
        public bool CanPlayCard(
            CardSO card,
            FieldState fieldState,
            GameRulesSO gameRules)
        {
            if (card == null) return false;
            if (fieldState.IsEmpty) return true;

            // Phase 2: 縛りルールチェック
            if (fieldState.IsBindingActive(gameRules))
            {
                var bindingSuit = fieldState.GetBindingSuit(gameRules);
                if (bindingSuit.HasValue && card.CardSuit != bindingSuit.Value)
                {
                    return false; // 縛り中は同じスートのみ
                }
            }

            // 基本的な強度比較
            return card.GetStrength() > fieldState.Strength;

            // Phase 2以降: 革命などのルール判定を追加予定
            // if (gameRules.IsRevolutionEnabled && fieldState.IsRevolutionActive)
            //     return card.GetStrength() < fieldState.Strength;
        }

        /// <summary>
        /// カードが手札に含まれているか検証
        /// </summary>
        /// <param name="card">チェックするカード</param>
        /// <param name="hand">手札</param>
        /// <returns>True if card is in hand, false otherwise</returns>
        public bool IsCardInHand(CardSO card, PlayerHandSO hand)
        {
            return card != null && hand != null && hand.HasCard(card);
        }
    }
}

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
            if (hand.Cards == null) return new List<CardSO>();

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

            // スペ3返し：ジョーカー単体に対してスペード3を出せる（縛り無視）
            // Phase 1: 複数枚出しがないため、CurrentCardがJokerであればJoker単体プレイと判定
            if (gameRules.IsSpade3ReturnEnabled &&
                fieldState.CurrentCard != null &&
                fieldState.CurrentCard.IsJoker &&
                card.CardSuit == CardSO.Suit.Spade &&
                card.Rank == 3)
            {
                return true;
            }

            // Phase 2: 縛りルールチェック
            if (fieldState.IsBindingActive(gameRules))
            {
                var bindingSuit = fieldState.GetBindingSuit(gameRules);
                if (bindingSuit.HasValue && card.CardSuit != bindingSuit.Value)
                {
                    return false; // 縛り中は同じスートのみ
                }
            }

            // Phase 1.5: 革命を考慮した強度比較
            // GetEffectiveRevolution() は革命 XOR 11バックを返す
            bool isRevolution = fieldState.GetEffectiveRevolution();
            int cardStrength = card.GetStrength(isRevolution);
            int fieldStrength = fieldState.Strength; // Strength プロパティは既に GetEffectiveRevolution() を使っている

            // 強度比較：革命を考慮した強さ値同士を比較（常に > で比較）
            return cardStrength > fieldStrength;
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

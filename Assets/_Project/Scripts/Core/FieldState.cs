using System.Collections.Generic;
using Daifugo.Data;

namespace Daifugo.Core
{
    /// <summary>
    /// 場の状態を表すデータ構造
    /// Phase 1: 単一カードの履歴を保持（縛りルール対応）
    /// Phase 2以降: 複数枚出し、階段、革命状態などを追加予定
    /// </summary>
    public struct FieldState
    {
        // ========== Core Data ==========

        /// <summary>
        /// 場に出ている全カードの履歴（親が出してから現在まで）
        /// イミュータブル：常に新しいListを生成
        /// </summary>
        public readonly IReadOnlyList<CardSO> CardsInField;

        // ========== Private Constructor ==========

        /// <summary>
        /// プライベートコンストラクタ（Factory Methodから呼び出し）
        /// </summary>
        private FieldState(IReadOnlyList<CardSO> cards)
        {
            CardsInField = cards ?? new List<CardSO>();
        }

        // ========== Derived Properties ==========

        /// <summary>場が空か</summary>
        public bool IsEmpty => CardsInField == null || CardsInField.Count == 0;

        /// <summary>現在のカード（最後に出されたカード）</summary>
        public CardSO CurrentCard => IsEmpty ? null : CardsInField[^1];

        /// <summary>場の強度（現在のカードの強度）</summary>
        public int Strength => CurrentCard?.GetStrength() ?? 0;

        // ========== Factory Methods ==========

        /// <summary>
        /// 空の場を生成
        /// </summary>
        public static FieldState Empty() => new FieldState(new List<CardSO>());

        /// <summary>
        /// 場にカードを追加（イミュータブル：新しいFieldStateを返す）
        /// </summary>
        /// <param name="current">現在の場の状態</param>
        /// <param name="card">追加するカード</param>
        /// <returns>カードが追加された新しい場の状態</returns>
        public static FieldState AddCard(FieldState current, CardSO card)
        {
            var newList = new List<CardSO>(current.CardsInField) { card };
            return new FieldState(newList);
        }

        // ========== Phase 2: 縛りルール ==========

        /// <summary>
        /// 縛りが発動しているか判定
        /// 最後の2枚が同じスートの場合に縛り発動
        /// </summary>
        /// <param name="rules">ゲームルール設定</param>
        /// <returns>縛りが発動している場合true</returns>
        public bool IsBindingActive(GameRulesSO rules)
        {
            if (!rules.IsBindEnabled) return false;
            if (CardsInField.Count < 2) return false;

            // 最後の2枚が同じスートか
            return CardsInField[^1].CardSuit == CardsInField[^2].CardSuit;
        }

        /// <summary>
        /// 縛られているスートを取得
        /// </summary>
        /// <param name="rules">ゲームルール設定</param>
        /// <returns>縛りが発動している場合はスート、それ以外はnull</returns>
        public CardSO.Suit? GetBindingSuit(GameRulesSO rules)
        {
            if (!IsBindingActive(rules)) return null;
            return CardsInField[^1].CardSuit;
        }

        // ========== Phase 2以降で追加予定 ==========

        // /// <summary>革命が発動中か（ゲーム状態から取得）</summary>
        // public bool IsRevolutionActive { get; private set; }
        //
        // /// <summary>複数枚出しのパターン</summary>
        // public PlayPattern Pattern { get; private set; }
        //
        // /// <summary>階段出しのパターン</summary>
        // public bool IsSequence { get; private set; }
    }
}

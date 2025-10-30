using Daifugo.Data;

namespace Daifugo.Core
{
    /// <summary>
    /// 場の状態を表すデータ構造
    /// Phase 1: 単一カードの強度のみ
    /// Phase 2以降: 複数枚、階段、革命状態などを追加予定
    /// </summary>
    public struct FieldState
    {
        // ========== Phase 1実装 ==========

        /// <summary>場が空か</summary>
        public bool IsEmpty { get; private set; }

        /// <summary>場のカード（Phase 1: 単一カードのみ）</summary>
        public CardSO CurrentCard { get; private set; }

        /// <summary>場の強度</summary>
        public int Strength { get; private set; }

        // ========== Phase 2以降で追加予定 ==========

        // /// <summary>場に出ているカード（複数枚出し対応）</summary>
        // public List<CardSO> CurrentCards { get; private set; }
        //
        // /// <summary>出し方のパターン</summary>
        // public PlayPattern Pattern { get; private set; }
        //
        // /// <summary>革命が発動中か（ゲーム状態から取得）</summary>
        // public bool IsRevolutionActive { get; private set; }
        //
        // /// <summary>縛りが発動中か</summary>
        // public bool IsBindActive { get; private set; }
        //
        // /// <summary>縛りのスート</summary>
        // public CardSuit? BindSuit { get; private set; }

        // ========== Factory Methods ==========

        /// <summary>
        /// 空の場を生成
        /// </summary>
        public static FieldState Empty() => new FieldState
        {
            IsEmpty = true,
            CurrentCard = null,
            Strength = 0
        };

        /// <summary>
        /// カードが出ている場を生成（Phase 1: 単一カード）
        /// </summary>
        /// <param name="card">場に出ているカード</param>
        /// <returns>カードが出ている場の状態</returns>
        public static FieldState FromCard(CardSO card) => new FieldState
        {
            IsEmpty = false,
            CurrentCard = card,
            Strength = card?.GetStrength() ?? 0
        };

        // Phase 2以降: 複数枚出し対応
        // public static FieldState FromCards(List<CardSO> cards, PlayPattern pattern) { }
    }

    // Phase 2以降で追加予定
    // /// <summary>
    // /// 出し方のパターン
    // /// </summary>
    // public enum PlayPattern
    // {
    //     Single,      // 1枚出し
    //     Pair,        // ペア
    //     Triple,      // スリーカード
    //     Quad,        // 4枚出し
    //     Straight     // 階段
    // }
}

using System.Collections.Generic;
using System.Linq;
using Daifugo.Data;

namespace Daifugo.Core
{
    /// <summary>
    /// Represents a single card play (one or more cards played together)
    /// Phase 1.5: Supports multiple cards played simultaneously
    /// </summary>
    public struct CardPlay
    {
        /// <summary>Cards played in this play</summary>
        public readonly IReadOnlyList<CardSO> Cards;

        /// <summary>Player who made this play</summary>
        public readonly int PlayerID;

        /// <summary>
        /// Creates a new CardPlay
        /// </summary>
        /// <param name="cards">Cards played</param>
        /// <param name="playerID">Player ID</param>
        public CardPlay(List<CardSO> cards, int playerID)
        {
            Cards = cards ?? new List<CardSO>();
            PlayerID = playerID;
        }

        /// <summary>Number of cards in this play</summary>
        public int Count => Cards.Count;
    }

    /// <summary>
    /// 場の状態を表すデータ構造
    /// Phase 1: 単一カードの履歴を保持（縛りルール対応）
    /// Phase 1.5: 複数枚出し、階段、革命状態を追加
    /// </summary>
    public struct FieldState
    {
        // ========== Core Data ==========

        /// <summary>
        /// 場に出されたプレイの履歴（親が出してから現在まで）
        /// Phase 1.5: CardPlay のリストに変更
        /// イミュータブル：常に新しいListを生成
        /// </summary>
        public readonly IReadOnlyList<CardPlay> PlayHistory;

        /// <summary>
        /// 革命が発動中か（4枚出しによる永続的な強さ逆転）
        /// ゲーム終了まで継続する
        /// </summary>
        public readonly bool IsRevolutionActive;

        /// <summary>
        /// 一時革命が発動中か（11バックによる一時的な強さ逆転）
        /// 場が流れるまで継続する
        /// </summary>
        public readonly bool IsTemporaryRevolution;

        // ========== Private Constructor ==========

        /// <summary>
        /// プライベートコンストラクタ（Factory Methodから呼び出し）
        /// </summary>
        private FieldState(IReadOnlyList<CardPlay> playHistory, bool isRevolutionActive, bool isTemporaryRevolution)
        {
            PlayHistory = playHistory ?? new List<CardPlay>();
            IsRevolutionActive = isRevolutionActive;
            IsTemporaryRevolution = isTemporaryRevolution;
        }

        // ========== Derived Properties ==========

        /// <summary>場が空か</summary>
        public bool IsEmpty => PlayHistory == null || PlayHistory.Count == 0;

        /// <summary>最後のプレイ（複数枚出しに対応）</summary>
        public CardPlay? CurrentPlay => IsEmpty ? null : PlayHistory[^1];

        /// <summary>現在のカード（最後に出されたカードの最後の1枚）</summary>
        public CardSO CurrentCard
        {
            get
            {
                if (IsEmpty) return null;
                var lastPlay = PlayHistory[^1];
                return lastPlay.Cards.Count > 0 ? lastPlay.Cards[^1] : null;
            }
        }

        /// <summary>
        /// 場の強度（現在のプレイの強度、革命を考慮）
        /// ジョーカーを含む場合は非ジョーカーカードの強度を使用
        /// </summary>
        public int Strength
        {
            get
            {
                if (!CurrentPlay.HasValue) return 0;

                var cards = CurrentPlay.Value.Cards;
                bool isRevolution = GetEffectiveRevolution();

                // Find first non-Joker card to determine strength
                var nonJoker = cards.FirstOrDefault(c => !c.IsJoker);

                // If all cards are Jokers, use Joker strength (16)
                if (nonJoker == null)
                {
                    return cards[0].GetStrength(isRevolution); // Joker strength is always 16
                }

                // Use non-Joker card strength (Jokers act as wildcards)
                return nonJoker.GetStrength(isRevolution);
            }
        }

        /// <summary>
        /// Phase 1 互換性: 場に出ている全カードを単一リストとして取得
        /// </summary>
        public IReadOnlyList<CardSO> CardsInField
        {
            get
            {
                var allCards = new List<CardSO>();
                foreach (var play in PlayHistory)
                {
                    allCards.AddRange(play.Cards);
                }
                return allCards;
            }
        }

        /// <summary>
        /// 実効的な革命状態を取得（革命 XOR 11バック）
        /// </summary>
        public bool GetEffectiveRevolution()
        {
            return IsRevolutionActive ^ IsTemporaryRevolution;
        }

        /// <summary>
        /// 最後のプレイのパターンを取得
        /// </summary>
        public PlayPattern GetLastPlayPattern()
        {
            if (IsEmpty) return PlayPattern.Invalid;
            var detector = new PlayPatternDetector();
            return detector.DetectPattern(CurrentPlay.Value.Cards.ToList());
        }

        /// <summary>
        /// 最後のプレイのカード枚数を取得
        /// </summary>
        public int GetLastPlayCount()
        {
            return CurrentPlay?.Count ?? 0;
        }

        // ========== Factory Methods ==========

        /// <summary>
        /// 空の場を生成
        /// </summary>
        public static FieldState Empty() =>
            new FieldState(new List<CardPlay>(), isRevolutionActive: false, isTemporaryRevolution: false);

        /// <summary>
        /// 空の場を生成（革命状態を指定）
        /// </summary>
        /// <param name="isRevolutionActive">革命が発動中か</param>
        public static FieldState EmptyWithRevolution(bool isRevolutionActive) =>
            new FieldState(new List<CardPlay>(), isRevolutionActive, isTemporaryRevolution: false);

        /// <summary>
        /// 場にカードを追加（Phase 1 互換）
        /// イミュータブル：新しいFieldStateを返す
        /// </summary>
        /// <param name="current">現在の場の状態</param>
        /// <param name="card">追加するカード</param>
        /// <param name="playerID">プレイヤーID</param>
        /// <param name="activates11Back">11バックルールが発動するか</param>
        /// <returns>カードが追加された新しい場の状態</returns>
        public static FieldState AddCard(FieldState current, CardSO card, int playerID = 0, bool activates11Back = false)
        {
            // 単一カードを CardPlay として追加
            var cards = new List<CardSO> { card };
            var newPlay = new CardPlay(cards, playerID);

            var newHistory = new List<CardPlay>(current.PlayHistory) { newPlay };

            // 11バック発動時は一時革命状態を反転
            bool newTemporaryRevolution = activates11Back ? !current.IsTemporaryRevolution : current.IsTemporaryRevolution;

            return new FieldState(newHistory, current.IsRevolutionActive, newTemporaryRevolution);
        }

        /// <summary>
        /// 場に複数枚のカードを追加（Phase 1.5）
        /// イミュータブル：新しいFieldStateを返す
        /// </summary>
        /// <param name="current">現在の場の状態</param>
        /// <param name="cards">追加するカードのリスト</param>
        /// <param name="playerID">プレイヤーID</param>
        /// <param name="activatesRevolution">革命が発動するか（4枚出し）</param>
        /// <param name="activates11Back">11バックルールが発動するか</param>
        /// <returns>カードが追加された新しい場の状態</returns>
        public static FieldState AddCards(FieldState current, List<CardSO> cards, int playerID, bool activatesRevolution = false, bool activates11Back = false)
        {
            var newPlay = new CardPlay(cards, playerID);
            var newHistory = new List<CardPlay>(current.PlayHistory) { newPlay };

            // 革命発動時は永続的な革命状態を更新
            bool newRevolutionActive = activatesRevolution || current.IsRevolutionActive;

            // 11バック発動時は一時革命状態を反転
            bool newTemporaryRevolution = activates11Back ? !current.IsTemporaryRevolution : current.IsTemporaryRevolution;

            return new FieldState(newHistory, newRevolutionActive, newTemporaryRevolution);
        }

        // ========== Phase 1.5: 縛りルール ==========

        /// <summary>
        /// 縛りが発動しているか判定
        /// Phase 1.5: 最後の2つのプレイの最後のカードが同じスートの場合に縛り発動
        /// ジョーカーは縛りに影響しない（スートを持たないカードとして扱う）
        /// </summary>
        /// <param name="rules">ゲームルール設定</param>
        /// <returns>縛りが発動している場合true</returns>
        public bool IsBindingActive(GameRulesSO rules)
        {
            if (!rules.IsBindEnabled) return false;
            if (PlayHistory.Count < 2) return false;

            // 最後の2つのプレイの最後のカードを取得
            var lastPlay = PlayHistory[^1];
            var secondLastPlay = PlayHistory[^2];

            if (lastPlay.Cards.Count == 0 || secondLastPlay.Cards.Count == 0) return false;

            CardSO lastCard = lastPlay.Cards[^1];
            CardSO secondLastCard = secondLastPlay.Cards[^1];

            // Jokerは縛りに影響しない
            if (lastCard.IsJoker || secondLastCard.IsJoker) return false;

            // 最後の2つのプレイの最後のカードが同じスートか
            return lastCard.CardSuit == secondLastCard.CardSuit;
        }

        /// <summary>
        /// 縛られているスートを取得
        /// </summary>
        /// <param name="rules">ゲームルール設定</param>
        /// <returns>縛りが発動している場合はスート、それ以外はnull</returns>
        public CardSO.Suit? GetBindingSuit(GameRulesSO rules)
        {
            if (!IsBindingActive(rules)) return null;

            var lastPlay = PlayHistory[^1];
            if (lastPlay.Cards.Count == 0) return null;

            return lastPlay.Cards[^1].CardSuit;
        }
    }
}

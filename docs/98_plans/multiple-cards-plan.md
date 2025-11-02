# Phase 1.5: 複数枚出し機能 - 技術実装計画

## 目的

multiple-cards-spec.mdとclarifications.md（C-015〜C-026）に基づき、複数枚出し機能の技術実装の詳細を計画する。

---

## アーキテクチャ概要

### 基本方針

**Phase 1のアーキテクチャを拡張、破壊的変更は避ける**

Phase 1の実装を活かしながら、以下の方針で拡張：
1. **既存のクラスを拡張**: FieldState、GameLogic、PlayableCardsCalculator
2. **新規クラスを追加**: PlayPatternDetector（プレイパターン判定）
3. **イミュータブル設計**: FieldStateはstruct、Factory Methodsで状態遷移
4. **純粋関数**: ロジックは副作用なし、テスト容易性を維持

### レイヤー構成（Phase 1から変更なし）

```
┌─────────────────────────────────────┐
│          UI Layer (UI Toolkit)       │
│  GameScreenUI, HandUI, CardUI        │
└──────────────┬──────────────────────┘
               │ EventChannel
┌──────────────┴──────────────────────┐
│         Event Layer                  │
│  OnGameStarted, OnTurnChanged, ...   │
└──────────────┬──────────────────────┘
               │ Subscribe/Raise
┌──────────────┴──────────────────────┐
│       Game Logic Layer               │
│  GameManager, TurnManager, ...       │
│  + PlayPatternDetector (新規)       │
└──────────────┬──────────────────────┘
               │ Access
┌──────────────┴──────────────────────┐
│         Data Layer                   │
│  CardSO, DeckSO, PlayerHandSO        │
│  + PlayPattern enum (新規)          │
└─────────────────────────────────────┘
```

---

## データ層の拡張

### PlayPattern（enum - 新規）

**責務**: プレイパターンの種類を表現

**定義**:
```csharp
namespace Daifugo.Core
{
    /// <summary>
    /// Play pattern types for card combinations
    /// </summary>
    public enum PlayPattern
    {
        /// <summary>Single card (1 card)</summary>
        Single,

        /// <summary>Pair (2 cards of same rank)</summary>
        Pair,

        /// <summary>Triple (3 cards of same rank)</summary>
        Triple,

        /// <summary>Quadruple (4 cards of same rank, triggers revolution)</summary>
        Quadruple,

        /// <summary>Sequence (3+ cards in consecutive rank, same suit)</summary>
        Sequence,

        /// <summary>Invalid combination</summary>
        Invalid
    }
}
```

**ファイル**: `Assets/_Project/Scripts/Core/PlayPattern.cs`

---

### FieldState（struct - 拡張）

**責務**: 場の状態を保持（Phase 1から拡張）

**Phase 1との違い**:
- `CardSO CurrentCard` → `IReadOnlyList<CardSO> CardsInField`（カード履歴を保持）
- `IsRevolutionActive` 追加（永続革命）
- `IsTemporaryRevolution` 追加（11バック）

**拡張後の定義**:
```csharp
namespace Daifugo.Core
{
    /// <summary>
    /// Represents the current field state
    /// Phase 1.5: Supports multiple cards, revolution tracking, binding detection
    /// </summary>
    public struct FieldState
    {
        // ========== Core Data ==========

        /// <summary>
        /// All cards currently in field (card play history from parent onwards)
        /// Immutable: Always create new List
        /// </summary>
        public readonly IReadOnlyList<CardSO> CardsInField;

        /// <summary>
        /// Permanent revolution active (until game end)
        /// Triggered by quadruple play (4 cards)
        /// </summary>
        public readonly bool IsRevolutionActive;

        /// <summary>
        /// Temporary revolution active (until field reset)
        /// Triggered by 11-back rule (J card)
        /// </summary>
        public readonly bool IsTemporaryRevolution;

        // ========== Private Constructor ==========

        private FieldState(IReadOnlyList<CardSO> cards, bool isRevolution, bool isTemporaryRevolution)
        {
            CardsInField = cards ?? new List<CardSO>();
            IsRevolutionActive = isRevolution;
            IsTemporaryRevolution = isTemporaryRevolution;
        }

        // ========== Derived Properties ==========

        /// <summary>Is field empty</summary>
        public bool IsEmpty => CardsInField == null || CardsInField.Count == 0;

        /// <summary>Current card (last card played)</summary>
        public CardSO CurrentCard => IsEmpty ? null : CardsInField[^1];

        /// <summary>Field strength (strength of current card)</summary>
        public int Strength => CurrentCard?.GetStrength(GetEffectiveRevolution()) ?? 0;

        /// <summary>
        /// Effective revolution state (permanent XOR temporary)
        /// </summary>
        public bool GetEffectiveRevolution()
        {
            return IsRevolutionActive ^ IsTemporaryRevolution;
        }

        // ========== Factory Methods ==========

        /// <summary>
        /// Creates empty field
        /// </summary>
        public static FieldState Empty()
        {
            return new FieldState(new List<CardSO>(), isRevolution: false, isTemporaryRevolution: false);
        }

        /// <summary>
        /// Creates field with revolution state only (for game start after revolution)
        /// </summary>
        public static FieldState EmptyWithRevolution(bool isRevolution)
        {
            return new FieldState(new List<CardSO>(), isRevolution: isRevolution, isTemporaryRevolution: false);
        }

        /// <summary>
        /// Adds a single card to field (Phase 1 compatibility)
        /// </summary>
        public static FieldState AddCard(
            FieldState current,
            CardSO card,
            bool activatesRevolution = false,
            bool activates11Back = false)
        {
            var newList = new List<CardSO>(current.CardsInField) { card };

            // Update revolution state
            bool newRevolution = current.IsRevolutionActive || activatesRevolution;
            bool newTemporaryRevolution = activates11Back ? !current.IsTemporaryRevolution : current.IsTemporaryRevolution;

            return new FieldState(newList, newRevolution, newTemporaryRevolution);
        }

        /// <summary>
        /// Adds multiple cards to field (Phase 1.5)
        /// </summary>
        public static FieldState AddCards(
            FieldState current,
            List<CardSO> cards,
            bool activatesRevolution = false,
            bool activates11Back = false)
        {
            var newList = new List<CardSO>(current.CardsInField);
            newList.AddRange(cards);

            // Update revolution state
            bool newRevolution = current.IsRevolutionActive || activatesRevolution;
            bool newTemporaryRevolution = activates11Back ? !current.IsTemporaryRevolution : current.IsTemporaryRevolution;

            return new FieldState(newList, newRevolution, newTemporaryRevolution);
        }

        // ========== Phase 1.5: Play Pattern Detection ==========

        /// <summary>
        /// Gets the play pattern of the last play
        /// </summary>
        public PlayPattern GetLastPlayPattern()
        {
            if (IsEmpty) return PlayPattern.Invalid;

            // Detect pattern from cards in field
            // For now, simplified: assume last N cards form the pattern
            // Actual implementation in PlayPatternDetector
            return PlayPattern.Single; // Placeholder
        }

        /// <summary>
        /// Gets number of cards in last play
        /// </summary>
        public int GetLastPlayCount()
        {
            // Implementation depends on how we track play boundaries
            // For Phase 1.5, we need to know where last play starts
            // Option: Store play boundaries in FieldState
            // For now, placeholder
            return IsEmpty ? 0 : 1;
        }

        // ========== Phase 1.5: Binding Rule ==========

        /// <summary>
        /// Checks if binding is active
        /// Last 2 plays are all same suit
        /// </summary>
        public bool IsBindingActive(GameRulesSO rules)
        {
            if (!rules.IsBindEnabled) return false;
            if (CardsInField.Count < 2) return false;

            // Get last 2 plays
            // Implementation requires tracking play boundaries
            // Placeholder for now
            return false;
        }

        /// <summary>
        /// Gets binding suit
        /// </summary>
        public CardSO.Suit? GetBindingSuit(GameRulesSO rules)
        {
            if (!IsBindingActive(rules)) return null;
            return CardsInField[^1].CardSuit;
        }
    }
}
```

**課題**: 場のカード履歴から「最後のプレイ」と「その前のプレイ」を区別する方法

**解決策（オプション）**:
1. **プレイ境界を記録**: `List<int> playBoundaries`（各プレイの開始インデックス）
2. **プレイごとにFieldStateを完全リセット**: 最後の1プレイのみ保持
3. **別のデータ構造**: `List<CardPlay>` where `CardPlay { List<CardSO> Cards }`

**推奨**: オプション3（明確、テスト容易）

**修正後の設計**:
```csharp
public struct CardPlay
{
    public readonly IReadOnlyList<CardSO> Cards;
    public readonly int PlayerID;

    public CardPlay(List<CardSO> cards, int playerID)
    {
        Cards = cards;
        PlayerID = playerID;
    }
}

public struct FieldState
{
    public readonly IReadOnlyList<CardPlay> PlayHistory;  // 最大2プレイ保持（縛り判定用）
    public readonly bool IsRevolutionActive;
    public readonly bool IsTemporaryRevolution;

    public CardPlay? CurrentPlay => PlayHistory.Count > 0 ? PlayHistory[^1] : null;
    public bool IsEmpty => PlayHistory.Count == 0;
}
```

**最終設計決定**: CardPlayを導入し、PlayHistoryで管理

---

### CardSO（既存 - 拡張）

**責務**: カードデータを保持（Phase 1から拡張）

**拡張内容**:
```csharp
/// <summary>
/// Gets card strength
/// Phase 1.5: Supports revolution parameter
/// </summary>
/// <param name="isRevolution">Whether revolution is active</param>
/// <returns>Card strength value</returns>
public int GetStrength(bool isRevolution = false)
{
    // Joker is always strongest
    if (IsJoker) return 16;

    if (isRevolution)
    {
        // Revolution: weak cards become strong
        if (Rank == 2) return 2;      // 2 is weakest (except Joker)
        if (Rank == 1) return 3;      // A is 3
        return 18 - Rank;             // 3 → 15, 4 → 14, ..., K(13) → 5
    }
    else
    {
        // Normal: Phase 1 logic unchanged
        if (Rank == 2) return 15;     // 2 is strongest (except Joker)
        if (Rank == 1) return 14;     // A is 14
        return Rank;                   // 3-13 unchanged
    }
}
```

**ファイル**: `Assets/_Project/Scripts/Data/CardSO.cs`（既存ファイルに追加）

---

## ロジック層の拡張

### PlayPatternDetector（純粋クラス - 新規）

**責務**: カード組み合わせからプレイパターンを判定

**実装**:
```csharp
namespace Daifugo.Core
{
    /// <summary>
    /// Detects play pattern from card combination
    /// Pure C# class for testability
    /// </summary>
    public class PlayPatternDetector
    {
        /// <summary>
        /// Detects pattern from cards
        /// Priority: Same rank → Sequence → Invalid
        /// </summary>
        public PlayPattern DetectPattern(List<CardSO> cards)
        {
            if (cards == null || cards.Count == 0)
                return PlayPattern.Invalid;

            // Single card
            if (cards.Count == 1)
                return PlayPattern.Single;

            // Same rank check (Pair/Triple/Quadruple)
            if (IsAllSameRank(cards))
            {
                return cards.Count switch
                {
                    2 => PlayPattern.Pair,
                    3 => PlayPattern.Triple,
                    4 => PlayPattern.Quadruple,
                    _ => PlayPattern.Invalid  // 5+ not supported in Phase 1.5
                };
            }

            // Sequence check
            if (IsSequence(cards))
                return PlayPattern.Sequence;

            return PlayPattern.Invalid;
        }

        /// <summary>
        /// Checks if all cards have same rank
        /// </summary>
        private bool IsAllSameRank(List<CardSO> cards)
        {
            if (cards.Count == 0) return false;

            int firstRank = cards[0].Rank;
            return cards.All(c => !c.IsJoker && c.Rank == firstRank);
        }

        /// <summary>
        /// Checks if cards form a sequence
        /// Requirements: 3+ cards, same suit, consecutive ranks
        /// Phase 1.5: No Joker, no wrap-around (K-A-2)
        /// </summary>
        public bool IsSequence(List<CardSO> cards)
        {
            if (cards.Count < 3) return false;

            // No Joker in sequence (Phase 1.5)
            if (cards.Any(c => c.IsJoker)) return false;

            // All same suit
            var suit = cards[0].CardSuit;
            if (cards.Any(c => c.CardSuit != suit)) return false;

            // Sort by rank
            var sorted = cards.OrderBy(c => c.Rank).ToList();

            // Check consecutive
            for (int i = 1; i < sorted.Count; i++)
            {
                if (sorted[i].Rank != sorted[i - 1].Rank + 1)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets sequence strength
        /// Normal: Max card strength
        /// Revolution: Min card strength (but still compare using > operator)
        /// </summary>
        public int GetSequenceStrength(List<CardSO> cards, bool isRevolution)
        {
            if (cards.Count == 0) return 0;

            if (isRevolution)
            {
                // Revolution: Compare by weakest card (min strength value)
                return cards.Min(c => c.GetStrength(true));
            }
            else
            {
                // Normal: Compare by strongest card (max strength value)
                return cards.Max(c => c.GetStrength(false));
            }
        }
    }
}
```

**ファイル**: `Assets/_Project/Scripts/Core/PlayPatternDetector.cs`（新規）

**テストファイル**: `Assets/_Project/Tests/Editor/Core/PlayPatternDetectorTests.cs`（新規）

---

### GameLogic（純粋クラス - 拡張）

**責務**: ゲームロジック（Phase 1から拡張）

**拡張内容**:
```csharp
namespace Daifugo.Core
{
    public class GameLogic
    {
        private readonly PlayableCardsCalculator calculator;
        private readonly PlayPatternDetector patternDetector;  // 新規
        private readonly GameRulesSO gameRules;

        public GameLogic(GameRulesSO gameRules)
        {
            calculator = new PlayableCardsCalculator();
            patternDetector = new PlayPatternDetector();  // 新規
            this.gameRules = gameRules;
        }

        // Phase 1: Single card play (existing)
        public CardPlayResult PlayCard(CardSO card, PlayerHandSO hand, FieldState fieldState)
        {
            // Existing implementation unchanged
            // ...
        }

        // Phase 1.5: Multiple cards play (new)
        public CardPlayResult PlayCards(List<CardSO> cards, PlayerHandSO hand, FieldState fieldState)
        {
            // Validation 1: Detect pattern
            PlayPattern pattern = patternDetector.DetectPattern(cards);
            if (pattern == PlayPattern.Invalid)
            {
                return CardPlayResult.Fail("Invalid card combination");
            }

            // Validation 2: Check if cards are in hand
            foreach (var card in cards)
            {
                if (!calculator.IsCardInHand(card, hand))
                {
                    return CardPlayResult.Fail($"Card {card.name} not in hand");
                }
            }

            // Validation 3: Check if cards can be played on field
            if (!CanPlayCards(cards, pattern, fieldState))
            {
                return CardPlayResult.Fail("Cannot play these cards on current field");
            }

            // Execute: Remove cards from hand
            foreach (var card in cards)
            {
                hand.RemoveCard(card);
            }

            // Check special rules
            bool shouldResetField = CheckSpecialRules_8Cut(cards);
            bool shouldActivate11Back = CheckSpecialRules_11Back(cards);
            bool shouldActivateRevolution = (pattern == PlayPattern.Quadruple);  // 4枚出しで革命

            // Update field state
            FieldState newFieldState = FieldState.AddCards(
                fieldState,
                cards,
                activatesRevolution: shouldActivateRevolution,
                activates11Back: shouldActivate11Back
            );

            // Check win condition
            bool isWin = hand.IsEmpty;

            // Check forbidden finish
            bool isForbiddenFinish = isWin && CheckForbiddenFinish(cards);

            return CardPlayResult.Success(
                newFieldState: newFieldState,
                isWin: isWin,
                shouldResetField: shouldResetField,
                shouldActivate11Back: shouldActivate11Back,
                shouldActivateRevolution: shouldActivateRevolution,
                isForbiddenFinish: isForbiddenFinish
            );
        }

        /// <summary>
        /// Checks if cards can be played on field
        /// </summary>
        private bool CanPlayCards(List<CardSO> cards, PlayPattern pattern, FieldState fieldState)
        {
            // Empty field: any pattern OK
            if (fieldState.IsEmpty) return true;

            // Get field pattern
            PlayPattern fieldPattern = fieldState.GetLastPlayPattern();
            int fieldCount = fieldState.GetLastPlayCount();

            // Pattern must match
            if (pattern != fieldPattern) return false;

            // Count must match (except Sequence can vary in length - NO, must match in Phase 1.5)
            if (cards.Count != fieldCount) return false;

            // Strength comparison
            bool isRevolution = fieldState.GetEffectiveRevolution();

            if (pattern == PlayPattern.Sequence)
            {
                // Sequence: Compare by sequence strength
                int ourStrength = patternDetector.GetSequenceStrength(cards, isRevolution);
                int fieldStrength = fieldState.Strength;  // Need to calculate field sequence strength
                return ourStrength > fieldStrength;
            }
            else
            {
                // Same rank: Compare by rank strength
                int ourStrength = cards[0].GetStrength(isRevolution);
                int fieldStrength = fieldState.Strength;
                return ourStrength > fieldStrength;
            }
        }

        /// <summary>
        /// Checks 8-cut rule for multiple cards
        /// </summary>
        private bool CheckSpecialRules_8Cut(List<CardSO> cards)
        {
            if (!gameRules.Is8CutEnabled) return false;

            // 8-cut: Any card with rank 8 triggers field reset
            return cards.Any(c => !c.IsJoker && c.Rank == 8);
        }

        /// <summary>
        /// Checks 11-back rule for multiple cards
        /// </summary>
        private bool CheckSpecialRules_11Back(List<CardSO> cards)
        {
            if (!gameRules.Is11BackEnabled) return false;

            // 11-back: Any card with rank 11 (J) triggers temporary revolution
            return cards.Any(c => !c.IsJoker && c.Rank == 11);
        }

        /// <summary>
        /// Checks forbidden finish for multiple cards
        /// </summary>
        private bool CheckForbiddenFinish(List<CardSO> cards)
        {
            if (!gameRules.IsForbiddenFinishEnabled) return false;

            // Forbidden finish: Any forbidden card in the combination
            foreach (var card in cards)
            {
                if (card.IsJoker) return true;
                if (card.Rank == 2) return true;
                if (card.Rank == 8) return true;
                if (card.CardSuit == CardSO.Suit.Spade && card.Rank == 3) return true;
            }

            return false;
        }
    }
}
```

**ファイル**: `Assets/_Project/Scripts/Core/GameLogic.cs`（既存ファイルに追加）

---

### CardPlayResult（struct - 拡張）

**Phase 1.5での拡張**:
```csharp
public struct CardPlayResult
{
    // Existing fields...
    public bool ShouldActivateRevolution { get; private set; }  // 新規: 革命発動フラグ

    public static CardPlayResult Success(
        FieldState newFieldState,
        bool isWin,
        bool shouldResetField,
        bool shouldActivate11Back = false,
        bool shouldActivateRevolution = false,  // 新規パラメータ
        bool isForbiddenFinish = false)
    {
        return new CardPlayResult
        {
            IsSuccess = true,
            IsWin = isWin,
            ShouldResetField = shouldResetField,
            ShouldActivate11Back = shouldActivate11Back,
            ShouldActivateRevolution = shouldActivateRevolution,  // 新規
            IsForbiddenFinish = isForbiddenFinish,
            NewFieldState = newFieldState,
            ErrorMessage = null
        };
    }
}
```

---

## AI層の拡張

### AIPlayerStrategy（純粋クラス - 拡張）

**Phase 1.5での拡張**:
```csharp
namespace Daifugo.AI
{
    public class AIPlayerStrategy
    {
        private readonly PlayableCardsCalculator calculator;
        private readonly PlayPatternDetector patternDetector;  // 新規
        private readonly GameRulesSO gameRules;

        public AIPlayerStrategy(GameRulesSO gameRules)
        {
            calculator = new PlayableCardsCalculator();
            patternDetector = new PlayPatternDetector();  // 新規
            this.gameRules = gameRules;
        }

        // Phase 1: Single card decision (existing)
        public CardSO DecideAction(PlayerHandSO hand, FieldState fieldState)
        {
            // Existing implementation unchanged
            // ...
        }

        // Phase 1.5: Multiple cards decision (new)
        public List<CardSO> DecideMultipleCardAction(PlayerHandSO hand, FieldState fieldState)
        {
            // Empty field: Play single weakest card (conserve pairs/sequences)
            if (fieldState.IsEmpty)
            {
                var singleCard = DecideAction(hand, fieldState);
                return singleCard != null ? new List<CardSO> { singleCard } : null;
            }

            // Get field pattern
            PlayPattern fieldPattern = fieldState.GetLastPlayPattern();
            int fieldCount = fieldState.GetLastPlayCount();

            // Find matching pattern in hand
            return fieldPattern switch
            {
                PlayPattern.Single => DecideSingleCard(hand, fieldState),
                PlayPattern.Pair => DecidePair(hand, fieldState, fieldCount),
                PlayPattern.Triple => DecideTriple(hand, fieldState, fieldCount),
                PlayPattern.Quadruple => DecideQuadruple(hand, fieldState, fieldCount),
                PlayPattern.Sequence => DecideSequence(hand, fieldState, fieldCount),
                _ => null  // Pass
            };
        }

        private List<CardSO> DecideSingleCard(PlayerHandSO hand, FieldState fieldState)
        {
            var card = DecideAction(hand, fieldState);
            return card != null ? new List<CardSO> { card } : null;
        }

        private List<CardSO> DecidePair(PlayerHandSO hand, FieldState fieldState, int count)
        {
            // Find all pairs in hand
            var pairs = FindPairsInHand(hand);

            // Filter playable pairs
            bool isRevolution = fieldState.GetEffectiveRevolution();
            int fieldStrength = fieldState.Strength;

            var playablePairs = pairs
                .Where(pair => pair[0].GetStrength(isRevolution) > fieldStrength)
                .ToList();

            if (playablePairs.Count == 0) return null;  // Pass

            // Phase 1.5 strategy: Play weakest pair (avoid playing 4-of-a-kind as pairs)
            return playablePairs
                .OrderBy(pair => pair[0].GetStrength(isRevolution))
                .First();
        }

        private List<List<CardSO>> FindPairsInHand(PlayerHandSO hand)
        {
            var pairs = new List<List<CardSO>>();

            // Group by rank
            var groups = hand.Cards
                .Where(c => !c.IsJoker)
                .GroupBy(c => c.Rank)
                .Where(g => g.Count() >= 2);

            foreach (var group in groups)
            {
                // Take first 2 cards (conserve 3rd/4th for potential triple/quadruple)
                pairs.Add(group.Take(2).ToList());
            }

            return pairs;
        }

        private List<CardSO> DecideTriple(PlayerHandSO hand, FieldState fieldState, int count)
        {
            // Find all triples in hand
            var triples = FindTriplesInHand(hand);

            // Filter playable triples
            bool isRevolution = fieldState.GetEffectiveRevolution();
            int fieldStrength = fieldState.Strength;

            var playableTriples = triples
                .Where(triple => triple[0].GetStrength(isRevolution) > fieldStrength)
                .ToList();

            if (playableTriples.Count == 0) return null;  // Pass

            // Phase 1.5 strategy: Play weakest triple (avoid playing 4-of-a-kind as triples)
            return playableTriples
                .OrderBy(triple => triple[0].GetStrength(isRevolution))
                .First();
        }

        private List<List<CardSO>> FindTriplesInHand(PlayerHandSO hand)
        {
            var triples = new List<List<CardSO>>();

            var groups = hand.Cards
                .Where(c => !c.IsJoker)
                .GroupBy(c => c.Rank)
                .Where(g => g.Count() >= 3);

            foreach (var group in groups)
            {
                // Take first 3 cards
                triples.Add(group.Take(3).ToList());
            }

            return triples;
        }

        private List<CardSO> DecideQuadruple(PlayerHandSO hand, FieldState fieldState, int count)
        {
            // Phase 1.5 strategy: NEVER play quadruple (avoid triggering revolution)
            // Instead, pass (AI doesn't know if revolution is beneficial)
            return null;
        }

        private List<CardSO> DecideSequence(PlayerHandSO hand, FieldState fieldState, int sequenceLength)
        {
            // Find all sequences of specified length in hand
            var sequences = FindSequencesInHand(hand, sequenceLength);

            // Filter playable sequences
            bool isRevolution = fieldState.GetEffectiveRevolution();
            int fieldStrength = fieldState.Strength;

            var playableSequences = sequences
                .Where(seq =>
                {
                    int strength = patternDetector.GetSequenceStrength(seq, isRevolution);
                    return strength > fieldStrength;
                })
                .ToList();

            if (playableSequences.Count == 0) return null;  // Pass

            // Phase 1.5 strategy: Play weakest sequence
            return playableSequences
                .OrderBy(seq => patternDetector.GetSequenceStrength(seq, isRevolution))
                .First();
        }

        private List<List<CardSO>> FindSequencesInHand(PlayerHandSO hand, int length)
        {
            var sequences = new List<List<CardSO>>();

            // Group by suit
            var suitGroups = hand.Cards
                .Where(c => !c.IsJoker)
                .GroupBy(c => c.CardSuit);

            foreach (var suitGroup in suitGroups)
            {
                // Sort by rank
                var sorted = suitGroup.OrderBy(c => c.Rank).ToList();

                // Find consecutive sequences of specified length
                for (int i = 0; i <= sorted.Count - length; i++)
                {
                    var candidate = sorted.Skip(i).Take(length).ToList();

                    // Check if consecutive
                    bool isConsecutive = true;
                    for (int j = 1; j < candidate.Count; j++)
                    {
                        if (candidate[j].Rank != candidate[j - 1].Rank + 1)
                        {
                            isConsecutive = false;
                            break;
                        }
                    }

                    if (isConsecutive)
                    {
                        sequences.Add(candidate);
                    }
                }
            }

            return sequences;
        }
    }
}
```

**ファイル**: `Assets/_Project/Scripts/AI/AIPlayerStrategy.cs`（既存ファイルに追加）

---

## UI層の拡張

### HandUI（C#クラス - 拡張）

**Phase 1.5での拡張**:
```csharp
namespace Daifugo.UI
{
    public class HandUI
    {
        private readonly VisualElement handContainer;
        private readonly PlayerHandSO handData;
        private readonly List<CardUI> cardUIElements = new();
        private List<CardUI> selectedCards = new();  // Phase 1: 単一 → Phase 1.5: 複数
        private List<CardSO> playableCards = new();

        public event Action OnSelectionChanged;

        // ... existing constructor ...

        public void Refresh()
        {
            handContainer.Clear();
            cardUIElements.Clear();

            foreach (var card in handData.Cards)
            {
                CardUI cardUI = new CardUI(card);
                cardUIElements.Add(cardUI);
                handContainer.Add(cardUI.Element);

                if (handData.PlayerID == 0)  // Human player only
                {
                    cardUI.Element.RegisterCallback<ClickEvent>(evt => OnCardClicked(cardUI));
                }
            }

            // Clear selection
            selectedCards.Clear();
        }

        private void OnCardClicked(CardUI cardUI)
        {
            // Phase 1.5: Multiple selection
            if (selectedCards.Contains(cardUI))
            {
                // Deselect
                selectedCards.Remove(cardUI);
                cardUI.SetSelected(false);
            }
            else
            {
                // Select
                selectedCards.Add(cardUI);
                cardUI.SetSelected(true);
            }

            OnSelectionChanged?.Invoke();
        }

        public List<CardSO> GetSelectedCards()
        {
            return selectedCards.Select(ui => ui.CardData).ToList();
        }

        public void ClearSelection()
        {
            foreach (var cardUI in selectedCards)
            {
                cardUI.SetSelected(false);
            }
            selectedCards.Clear();
            OnSelectionChanged?.Invoke();
        }

        public void HighlightPlayableCards(List<CardSO> newPlayableCards)
        {
            playableCards = newPlayableCards;

            foreach (var cardUI in cardUIElements)
            {
                if (playableCards.Contains(cardUI.CardData))
                {
                    cardUI.AddClass("card--playable");
                }
                else
                {
                    cardUI.RemoveClass("card--playable");
                }
            }
        }
    }
}
```

**ファイル**: `Assets/_Project/Scripts/UI/2D/HandUI.cs`（既存ファイルを修正）

---

### GameScreenUI（MonoBehaviour - 拡張）

**Phase 1.5での拡張**:
```csharp
private void OnPlayCardButtonClick()
{
    var selectedCards = playerHandUI.GetSelectedCards();
    if (selectedCards.Count == 0) return;

    // Phase 1: Single card
    if (selectedCards.Count == 1)
    {
        onPlayCardRequested.RaiseEvent(selectedCards[0]);
    }
    // Phase 1.5: Multiple cards
    else
    {
        // Validate pattern before requesting
        var detector = new PlayPatternDetector();
        var pattern = detector.DetectPattern(selectedCards);

        if (pattern == PlayPattern.Invalid)
        {
            ShowErrorMessage("無効な組み合わせです。同じランクまたは階段を選んでください");
            return;
        }

        // Request play via event (need new event channel for multiple cards)
        onPlayMultipleCardsRequested.RaiseEvent(selectedCards);
    }

    // Clear selection
    playerHandUI.ClearSelection();
}

private void ShowErrorMessage(string message)
{
    // Display error message in UI
    // Implementation: Show toast/label with error message
    Debug.LogWarning($"[GameScreenUI] {message}");
    // TODO: Visual feedback (red border, shake animation, etc.)
}
```

**新規EventChannel**:
```csharp
// ListCardEventChannelSO.cs (新規)
[CreateAssetMenu(menuName = "Events/ListCardEventChannel")]
public class ListCardEventChannelSO : GenericEventChannelSO<List<CardSO>> { }
```

**ファイル**:
- `Assets/_Project/Scripts/UI/2D/GameScreenUI.cs`（既存ファイルを修正）
- `Assets/_Project/Scripts/Events/ListCardEventChannelSO.cs`（新規）

---

## 実装順序

### フェーズ1: データ層とロジック基盤（1-2日）

**タスク**:
1. PlayPattern enum 作成
2. PlayPatternDetector 実装
3. PlayPatternDetectorTests 実装（TDD）
4. CardSO.GetStrength(bool isRevolution) 拡張
5. CardSOTests に革命テスト追加

**完了条件**:
- PlayPatternDetector が全パターンを正しく検出
- CardSO が革命中の強さを正しく計算
- 全テストがパス

---

### フェーズ2: FieldState拡張（1日）

**タスク**:
1. CardPlay struct 作成
2. FieldState を PlayHistory ベースに再設計
3. Factory Methods 実装（AddCard, AddCards, Empty, EmptyWithRevolution）
4. FieldStateTests 拡張（革命、履歴管理）

**完了条件**:
- FieldState が複数枚プレイを追跡
- 革命状態を正しく管理
- 全テストがパス

---

### フェーズ3: GameLogic拡張（1-2日）

**タスク**:
1. GameLogic.PlayCards() 実装
2. CardPlayResult に ShouldActivateRevolution 追加
3. 8切り、11バック、革命、禁止上がりの複数枚対応
4. GameLogicTests 拡張（複数枚シナリオ）

**完了条件**:
- PlayCards() が全パターンをサポート
- 特殊ルールが複数枚で正しく動作
- 全テストがパス

---

### フェーズ4: AI拡張（1日）

**タスク**:
1. AIPlayerStrategy.DecideMultipleCardAction() 実装
2. ペア、3枚、階段検出ロジック実装
3. AIPlayerStrategyTests 拡張

**完了条件**:
- AIが複数枚を正しく判断
- 革命を起こさない戦略が動作
- 全テストがパス

---

### フェーズ5: UI拡張（1-2日）

**タスク**:
1. HandUI を複数選択対応に拡張
2. ListCardEventChannelSO 作成
3. GameScreenUI を複数枚対応に拡張
4. エラーメッセージ表示機能追加
5. 革命表示UI追加

**完了条件**:
- 複数枚選択が動作
- エラーメッセージが表示される
- 革命マークが表示される

---

### フェーズ6: 統合とテスト（1-2日）

**タスク**:
1. GameManager を複数枚対応に拡張
2. AIController を複数枚対応に拡張
3. Inspector設定の更新
4. 統合テスト（手動プレイテスト）
5. バグ修正

**完了条件**:
- ゲームが開始から終了まで完走
- 全パターン（ペア、階段、革命）が動作
- 特殊ルールが正しく機能

---

### フェーズ7: 最終調整（0.5-1日）

**タスク**:
1. パフォーマンス最適化
2. UI/UXの微調整
3. 最終プレイテスト（5ゲーム連続完走）
4. ドキュメント更新

**完了条件**:
- 5ゲーム連続完走成功
- コーディング規約準拠
- Phase 1.5 完了

---

## 総実装期間

**推定**: 7-10日

- データ層・ロジック基盤: 1-2日
- FieldState拡張: 1日
- GameLogic拡張: 1-2日
- AI拡張: 1日
- UI拡張: 1-2日
- 統合とテスト: 1-2日
- 最終調整: 0.5-1日

---

## リスク管理

### 高リスク項目

1. **FieldStateの履歴管理**: PlayHistory の設計が複雑
   - 対策: 早期にプロトタイプ実装、テスト重視

2. **縛りルールの実装**: 最後の2プレイの判定が複雑
   - 対策: FieldState設計で明確に境界を定義

3. **革命中の強さ計算**: バグが多発しやすい
   - 対策: CardSO.GetStrength() を徹底的にテスト

### 中リスク項目

1. **AI戦略の複雑化**: ペア・階段の検出ロジック
   - 対策: シンプルな戦略に留める（Phase 2で高度化）

2. **UI の複数選択**: 選択状態の管理
   - 対策: Phase 1の実装を最大限活用

---

## 次のステップ

この実装計画を基に、以下を実行する：

- **/tasks**: 実装タスクへの詳細な分解
- **/implement**: タスクの順次実装

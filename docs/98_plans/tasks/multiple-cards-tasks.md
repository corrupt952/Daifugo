# Phase 1.5: 複数枚出し機能 - 実装タスク

## 目的

multiple-cards-plan.mdに基づき、実装を具体的なタスクに分解する。

各タスクは以下を満たす：
- **独立性**: 他のタスクに依存せず実装可能（依存がある場合は明記）
- **テスト可能性**: 完了を確認できる
- **明確な完了条件**: Done/Not Doneが明確
- **TDD推奨**: テストを先に書き、実装を後にする

---

## フェーズ1: データ層とロジック基盤（1-2日）

### Task 1.1: PlayPattern enum 作成

**ファイル**: `Assets/_Project/Scripts/Core/PlayPattern.cs`（新規）

**実装内容**:
```csharp
public enum PlayPattern
{
    Single,      // 1枚
    Pair,        // 2枚
    Triple,      // 3枚
    Quadruple,   // 4枚（革命発動）
    Sequence,    // 階段（3枚以上）
    Invalid      // 無効
}
```

**完了条件**:
- PlayPattern.csが作成され、コンパイルエラーなし
- 全パターンが定義されている

**依存**: なし

**推定時間**: 15分

---

### Task 1.2: PlayPatternDetectorTests 作成（TDD）

**ファイル**: `Assets/_Project/Tests/Editor/Core/PlayPatternDetectorTests.cs`（新規）

**実装内容**:
- 単体検出テスト
- ペア検出テスト（同じランク2枚）
- 3枚検出テスト（同じランク3枚）
- 4枚検出テスト（同じランク4枚）
- 階段検出テスト（同じスート、連続ランク、3枚以上）
- 無効パターン検出テスト
- 階段の強さ計算テスト（通常・革命）

**テストケース例**:
```csharp
[Test]
public void DetectPattern_Pair_ReturnsPair()
{
    // Arrange
    var cards = new List<CardSO> { TestHelpers.CreateCard(5, Suit.Spade), TestHelpers.CreateCard(5, Suit.Heart) };
    var detector = new PlayPatternDetector();

    // Act
    var pattern = detector.DetectPattern(cards);

    // Assert
    Assert.AreEqual(PlayPattern.Pair, pattern);
}

[Test]
public void DetectPattern_Sequence_ReturnsSequence()
{
    // Arrange
    var cards = new List<CardSO>
    {
        TestHelpers.CreateCard(3, Suit.Spade),
        TestHelpers.CreateCard(4, Suit.Spade),
        TestHelpers.CreateCard(5, Suit.Spade)
    };
    var detector = new PlayPatternDetector();

    // Act
    var pattern = detector.DetectPattern(cards);

    // Assert
    Assert.AreEqual(PlayPattern.Sequence, pattern);
}

[Test]
public void GetSequenceStrength_Normal_ReturnsMaxStrength()
{
    // Arrange
    var cards = new List<CardSO>
    {
        TestHelpers.CreateCard(3, Suit.Spade),  // Strength: 3
        TestHelpers.CreateCard(4, Suit.Spade),  // Strength: 4
        TestHelpers.CreateCard(5, Suit.Spade)   // Strength: 5
    };
    var detector = new PlayPatternDetector();

    // Act
    int strength = detector.GetSequenceStrength(cards, isRevolution: false);

    // Assert
    Assert.AreEqual(5, strength);  // Max strength
}

[Test]
public void GetSequenceStrength_Revolution_ReturnsMinStrength()
{
    // Arrange
    var cards = new List<CardSO>
    {
        TestHelpers.CreateCard(3, Suit.Spade),  // Revolution strength: 15
        TestHelpers.CreateCard(4, Suit.Spade),  // Revolution strength: 14
        TestHelpers.CreateCard(5, Suit.Spade)   // Revolution strength: 13
    };
    var detector = new PlayPatternDetector();

    // Act
    int strength = detector.GetSequenceStrength(cards, isRevolution: true);

    // Assert
    Assert.AreEqual(13, strength);  // Min strength in revolution
}
```

**完了条件**:
- 15以上のテストケースを作成
- 全テストが失敗する（Red状態、実装前）

**依存**: Task 1.1

**推定時間**: 1-2時間

---

### Task 1.3: PlayPatternDetector 実装

**ファイル**: `Assets/_Project/Scripts/Core/PlayPatternDetector.cs`（新規）

**実装内容**:
- DetectPattern() メソッド
- IsAllSameRank() メソッド
- IsSequence() メソッド
- GetSequenceStrength() メソッド

**完了条件**:
- PlayPatternDetector.csが作成され、コンパイルエラーなし
- Task 1.2の全テストがパス（Green状態）

**依存**: Task 1.2

**推定時間**: 1-2時間

---

### Task 1.4: CardSOTests 拡張（革命テスト追加）

**ファイル**: `Assets/_Project/Tests/Editor/Data/CardSOTests.cs`（既存ファイルに追加）

**実装内容**:
革命中の強さ計算テストを追加:

```csharp
[Test]
public void GetStrength_Revolution_3_Returns15()
{
    // Arrange
    var card = TestHelpers.CreateCard(3, Suit.Spade);

    // Act
    int strength = card.GetStrength(isRevolution: true);

    // Assert
    Assert.AreEqual(15, strength);  // 3 is strongest in revolution
}

[Test]
public void GetStrength_Revolution_2_Returns2()
{
    // Arrange
    var card = TestHelpers.CreateCard(2, Suit.Spade);

    // Act
    int strength = card.GetStrength(isRevolution: true);

    // Assert
    Assert.AreEqual(2, strength);  // 2 is weakest in revolution
}

[Test]
public void GetStrength_Revolution_Ace_Returns3()
{
    // Arrange
    var card = TestHelpers.CreateCard(1, Suit.Spade);  // Ace

    // Act
    int strength = card.GetStrength(isRevolution: true);

    // Assert
    Assert.AreEqual(3, strength);
}

[Test]
public void GetStrength_Revolution_King_Returns5()
{
    // Arrange
    var card = TestHelpers.CreateCard(13, Suit.Spade);  // King

    // Act
    int strength = card.GetStrength(isRevolution: true);

    // Assert
    Assert.AreEqual(5, strength);  // 18 - 13 = 5
}

[Test]
public void GetStrength_Revolution_Joker_Returns16()
{
    // Arrange
    var card = TestHelpers.CreateJoker();

    // Act
    int strength = card.GetStrength(isRevolution: true);

    // Assert
    Assert.AreEqual(16, strength);  // Joker is always strongest
}
```

**完了条件**:
- 5以上のテストケースを追加
- 全テストが失敗する（Red状態）

**依存**: なし

**推定時間**: 30分

---

### Task 1.5: CardSO.GetStrength(bool isRevolution) 実装

**ファイル**: `Assets/_Project/Scripts/Data/CardSO.cs`（既存ファイルに追加）

**実装内容**:
```csharp
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

**完了条件**:
- GetStrength()メソッドが実装されている
- Task 1.4の全テストがパス（Green状態）
- 既存のCardSOTestsも引き続きパス（リグレッションなし）

**依存**: Task 1.4

**推定時間**: 30分

---

## フェーズ2: FieldState拡張（1日）

### Task 2.1: CardPlay struct 作成

**ファイル**: `Assets/_Project/Scripts/Core/FieldState.cs`（既存ファイルに追加）

**実装内容**:
```csharp
/// <summary>
/// Represents a single card play (one or more cards played together)
/// </summary>
public struct CardPlay
{
    public readonly IReadOnlyList<CardSO> Cards;
    public readonly int PlayerID;

    public CardPlay(List<CardSO> cards, int playerID)
    {
        Cards = cards ?? new List<CardSO>();
        PlayerID = playerID;
    }

    public int Count => Cards.Count;
}
```

**完了条件**:
- CardPlay structが作成され、コンパイルエラーなし

**依存**: なし

**推定時間**: 15分

---

### Task 2.2: FieldState 再設計（PlayHistory ベース）

**ファイル**: `Assets/_Project/Scripts/Core/FieldState.cs`（既存ファイルを修正）

**実装内容**:
- `IReadOnlyList<CardPlay> PlayHistory` に変更
- `IsRevolutionActive`, `IsTemporaryRevolution` 追加
- Factory Methods 実装:
  - `Empty()`
  - `EmptyWithRevolution(bool)`
  - `AddCard()` - Phase 1互換
  - `AddCards()` - Phase 1.5新規
- Derived Properties:
  - `CurrentPlay`
  - `IsEmpty`
  - `GetEffectiveRevolution()`
  - `GetLastPlayPattern()`
  - `GetLastPlayCount()`

**完了条件**:
- FieldStateが再設計され、コンパイルエラーなし
- Phase 1の既存コードとの互換性を維持

**依存**: Task 2.1

**推定時間**: 2-3時間

---

### Task 2.3: FieldStateTests 拡張

**ファイル**: `Assets/_Project/Tests/Editor/Core/FieldStateTests.cs`（既存ファイルに追加）

**実装内容**:
新規テストケース:
- PlayHistory追跡テスト
- 革命状態管理テスト
- 11バック（一時革命）テスト
- XOR動作テスト（革命 + 11バック）
- AddCards()テスト

**テストケース例**:
```csharp
[Test]
public void AddCards_MultipleCards_TracksPlayHistory()
{
    // Arrange
    var field = FieldState.Empty();
    var cards = new List<CardSO>
    {
        TestHelpers.CreateCard(5, Suit.Spade),
        TestHelpers.CreateCard(5, Suit.Heart)
    };

    // Act
    var newField = FieldState.AddCards(field, cards, playerID: 0);

    // Assert
    Assert.AreEqual(1, newField.PlayHistory.Count);
    Assert.AreEqual(2, newField.CurrentPlay.Value.Cards.Count);
}

[Test]
public void GetEffectiveRevolution_RevolutionActive_11BackActive_ReturnsFalse()
{
    // Arrange
    var field = FieldState.Empty();
    field = FieldState.AddCard(field, TestHelpers.CreateCard(5, Suit.Spade),
        activatesRevolution: true, activates11Back: true);

    // Act
    bool effectiveRevolution = field.GetEffectiveRevolution();

    // Assert
    Assert.IsFalse(effectiveRevolution);  // true XOR true = false
}
```

**完了条件**:
- 10以上のテストケースを追加
- 全テストがパス

**依存**: Task 2.2

**推定時間**: 1-2時間

---

## フェーズ3: GameLogic拡張（1-2日）

### Task 3.1: CardPlayResult 拡張

**ファイル**: `Assets/_Project/Scripts/Core/GameLogic.cs`（既存ファイルを修正）

**実装内容**:
- `ShouldActivateRevolution` プロパティ追加
- Success() メソッドに `shouldActivateRevolution` パラメータ追加

**完了条件**:
- CardPlayResult structが拡張され、コンパイルエラーなし
- 既存のPhase 1コードに影響なし

**依存**: なし

**推定時間**: 15分

---

### Task 3.2: GameLogic.PlayCards() 実装（TDD）

**ファイル**: `Assets/_Project/Scripts/Core/GameLogic.cs`（既存ファイルに追加）

**実装内容**:
- PlayCards() メソッド実装
- CanPlayCards() メソッド実装
- 8切り複数枚対応
- 11バック複数枚対応
- 禁止上がり複数枚対応

**テストファイル**: `Assets/_Project/Tests/Editor/Core/GameLogicTests.cs`（追加）

**テストケース例**:
```csharp
[Test]
public void PlayCards_Pair_Success()
{
    // Arrange
    var hand = TestHelpers.CreateHandWithCards(
        TestHelpers.CreateCard(5, Suit.Spade),
        TestHelpers.CreateCard(5, Suit.Heart)
    );
    var field = FieldState.Empty();
    var logic = new GameLogic(TestHelpers.CreateDefaultRules());
    var cards = new List<CardSO> { hand.Cards[0], hand.Cards[1] };

    // Act
    var result = logic.PlayCards(cards, hand, field);

    // Assert
    Assert.IsTrue(result.IsSuccess);
    Assert.AreEqual(0, hand.CardCount);  // Cards removed
}

[Test]
public void PlayCards_Quadruple_ActivatesRevolution()
{
    // Arrange
    var hand = TestHelpers.CreateHandWithCards(
        TestHelpers.CreateCard(5, Suit.Spade),
        TestHelpers.CreateCard(5, Suit.Heart),
        TestHelpers.CreateCard(5, Suit.Diamond),
        TestHelpers.CreateCard(5, Suit.Club)
    );
    var field = FieldState.Empty();
    var logic = new GameLogic(TestHelpers.CreateDefaultRules());
    var cards = hand.Cards.ToList();

    // Act
    var result = logic.PlayCards(cards, hand, field);

    // Assert
    Assert.IsTrue(result.IsSuccess);
    Assert.IsTrue(result.ShouldActivateRevolution);
    Assert.IsTrue(result.NewFieldState.IsRevolutionActive);
}

[Test]
public void PlayCards_8Cut_ResetsField()
{
    // Arrange
    var hand = TestHelpers.CreateHandWithCards(
        TestHelpers.CreateCard(8, Suit.Spade),
        TestHelpers.CreateCard(8, Suit.Heart)
    );
    var field = FieldState.Empty();
    var logic = new GameLogic(TestHelpers.CreateDefaultRules());
    var cards = new List<CardSO> { hand.Cards[0], hand.Cards[1] };

    // Act
    var result = logic.PlayCards(cards, hand, field);

    // Assert
    Assert.IsTrue(result.IsSuccess);
    Assert.IsTrue(result.ShouldResetField);
}
```

**完了条件**:
- 20以上のテストケースを作成
- 全テストがパス
- Phase 1の既存テストも引き続きパス

**依存**: Task 3.1, Task 2.2（FieldState）

**推定時間**: 3-4時間

---

## フェーズ4: AI拡張（1日）

### Task 4.1: AIPlayerStrategy 拡張（TDD）

**ファイル**: `Assets/_Project/Scripts/AI/AIPlayerStrategy.cs`（既存ファイルに追加）

**実装内容**:
- DecideMultipleCardAction() メソッド
- FindPairsInHand() メソッド
- FindTriplesInHand() メソッド
- FindSequencesInHand() メソッド
- DecidePair(), DecideTriple(), DecideSequence() メソッド

**テストファイル**: `Assets/_Project/Tests/Editor/AI/AIPlayerStrategyTests.cs`（追加）

**テストケース例**:
```csharp
[Test]
public void DecideMultipleCardAction_FieldIsPair_PlaysPair()
{
    // Arrange
    var hand = TestHelpers.CreateHandWithCards(
        TestHelpers.CreateCard(5, Suit.Spade),
        TestHelpers.CreateCard(5, Suit.Heart),
        TestHelpers.CreateCard(7, Suit.Spade),
        TestHelpers.CreateCard(7, Suit.Heart)
    );
    var field = FieldState.AddCards(FieldState.Empty(),
        new List<CardSO> { TestHelpers.CreateCard(3, Suit.Spade), TestHelpers.CreateCard(3, Suit.Heart) },
        playerID: 1
    );
    var strategy = new AIPlayerStrategy(TestHelpers.CreateDefaultRules());

    // Act
    var cards = strategy.DecideMultipleCardAction(hand, field);

    // Assert
    Assert.IsNotNull(cards);
    Assert.AreEqual(2, cards.Count);  // Pair
    Assert.AreEqual(5, cards[0].Rank);  // Weakest playable pair
}

[Test]
public void DecideMultipleCardAction_Has4OfAKind_DoesNotPlayQuadruple()
{
    // Arrange: AI has 4 of a kind but field requires pair
    var hand = TestHelpers.CreateHandWithCards(
        TestHelpers.CreateCard(5, Suit.Spade),
        TestHelpers.CreateCard(5, Suit.Heart),
        TestHelpers.CreateCard(5, Suit.Diamond),
        TestHelpers.CreateCard(5, Suit.Club)
    );
    var field = FieldState.AddCards(FieldState.Empty(),
        new List<CardSO> { TestHelpers.CreateCard(3, Suit.Spade), TestHelpers.CreateCard(3, Suit.Heart) },
        playerID: 1
    );
    var strategy = new AIPlayerStrategy(TestHelpers.CreateDefaultRules());

    // Act
    var cards = strategy.DecideMultipleCardAction(hand, field);

    // Assert
    Assert.IsNotNull(cards);
    Assert.AreEqual(2, cards.Count);  // Only plays pair, not quadruple
}
```

**完了条件**:
- 15以上のテストケースを作成
- 全テストがパス
- Phase 1の既存テストも引き続きパス

**依存**: Task 3.2（GameLogic）

**推定時間**: 3-4時間

---

## フェーズ5: UI拡張（1-2日）

### Task 5.1: HandUI 複数選択対応

**ファイル**: `Assets/_Project/Scripts/UI/2D/HandUI.cs`（既存ファイルを修正）

**実装内容**:
- `selectedCard` → `selectedCards`（List）に変更
- OnCardClicked() で複数選択/解除ロジック
- GetSelectedCards() を List<CardSO> に変更

**完了条件**:
- HandUIが複数選択に対応
- コンパイルエラーなし
- 手動テストで複数枚選択が動作

**依存**: なし

**推定時間**: 1時間

---

### Task 5.2: ListCardEventChannelSO 作成

**ファイル**: `Assets/_Project/Scripts/Events/ListCardEventChannelSO.cs`（新規）

**実装内容**:
```csharp
[CreateAssetMenu(menuName = "Events/ListCardEventChannel")]
public class ListCardEventChannelSO : GenericEventChannelSO<List<CardSO>> { }
```

**アセット作成**: `Assets/_Project/ScriptableObjects/EventChannels/OnPlayMultipleCardsRequested.asset`

**完了条件**:
- ListCardEventChannelSO.csが作成され、コンパイルエラーなし
- アセットが作成されている

**依存**: なし

**推定時間**: 15分

---

### Task 5.3: GameScreenUI 複数枚対応

**ファイル**: `Assets/_Project/Scripts/UI/2D/GameScreenUI.cs`（既存ファイルを修正）

**実装内容**:
- OnPlayCardButtonClick() で複数枚判定
- ShowErrorMessage() メソッド追加
- 革命表示UI追加（Label「革命」）

**完了条件**:
- GameScreenUIが複数枚プレイに対応
- エラーメッセージが表示される
- コンパイルエラーなし

**依存**: Task 5.1, Task 5.2

**推定時間**: 2時間

---

### Task 5.4: 革命表示UI実装

**ファイル**: `Assets/_Project/UI/UXML/GameScreen.uxml`（既存ファイルを修正）

**実装内容**:
- 「革命」ラベルを追加
- 革命時に表示、通常時は非表示

**USS**: `Assets/_Project/UI/USS/GameScreen.uss`（スタイル追加）

**完了条件**:
- 革命マークが実装されている
- 革命時のみ表示される

**依存**: Task 5.3

**推定時間**: 30分

---

## フェーズ6: 統合とテスト（1-2日）

### Task 6.1: GameManager 複数枚対応

**ファイル**: `Assets/_Project/Scripts/Core/GameManager.cs`（既存ファイルを修正）

**実装内容**:
- OnPlayMultipleCardsRequested イベント購読
- HandlePlayMultipleCardsRequested() メソッド追加
- GameLogic.PlayCards() 呼び出し
- 革命状態の管理

**完了条件**:
- GameManagerが複数枚プレイに対応
- コンパイルエラーなし

**依存**: Task 3.2（GameLogic）, Task 5.2（ListCardEventChannelSO）

**推定時間**: 1-2時間

---

### Task 6.2: AIController 複数枚対応

**ファイル**: `Assets/_Project/Scripts/AI/AIController.cs`（既存ファイルを修正）

**実装内容**:
- AIPlayerStrategy.DecideMultipleCardAction() 呼び出し
- ExecuteAITurn() で複数枚プレイ対応

**完了条件**:
- AIControllerが複数枚プレイに対応
- コンパイルエラーなし

**依存**: Task 4.1（AIPlayerStrategy）, Task 6.1（GameManager）

**推定時間**: 1時間

---

### Task 6.3: Inspector設定の更新

**ファイル**: `Assets/_Project/Scenes/Main.unity`

**実装内容**:
- GameManager に OnPlayMultipleCardsRequested を設定
- AIController に 新しいEventChannelを設定
- GameScreenUI に新しいEventChannelを設定

**完了条件**:
- Inspector設定が完了
- NullReferenceExceptionが発生しない

**依存**: Task 6.1, Task 6.2

**推定時間**: 30分

---

### Task 6.4: 統合テスト（手動プレイテスト）

**実装内容**:
以下のシナリオを手動テスト:
1. ペア出し（場が空）
2. ペアに対してペア
3. 階段出し
4. 4枚出しで革命発動
5. 革命中のプレイ
6. 8切り（ペア）
7. 縛り（同じスートのペア2連続）
8. 11バック + 革命（XOR動作）
9. 禁止上がり（複数枚）
10. 無効な組み合わせ選択

**完了条件**:
- 全シナリオがエラーなく完走
- UI/UXが直感的
- バグリスト作成（発見した場合）

**依存**: Task 6.3

**推定時間**: 2-3時間

---

### Task 6.5: バグ修正

**実装内容**:
Task 6.4で発見されたバグを修正

**完了条件**:
- クリティカルバグが全て修正されている
- 再度手動テストでバグが再現しない

**依存**: Task 6.4

**推定時間**: 2-4時間（バグの数による）

---

## フェーズ7: 最終調整（0.5-1日）

### Task 7.1: パフォーマンス最適化

**実装内容**:
- PlayPatternDetector のアルゴリズム見直し
- FieldState の履歴管理を最適化（必要に応じて）
- AI判断ロジックの最適化

**完了条件**:
- カード選択のレスポンス < 50ms
- プレイパターン判定 < 100ms

**依存**: Task 6.5

**推定時間**: 1-2時間

---

### Task 7.2: UI/UXの微調整

**実装内容**:
- エラーメッセージの文言調整
- カード選択時のアニメーション調整
- 革命マークの見た目調整

**完了条件**:
- UIが見やすく、操作しやすい
- エラーメッセージが分かりやすい

**依存**: Task 6.5

**推定時間**: 1時間

---

### Task 7.3: 最終プレイテスト（5ゲーム連続完走）

**実装内容**:
5ゲーム連続でプレイし、以下を確認:
- ゲーム開始から終了までエラーなし
- 全パターン（ペア、階段、革命）が動作
- 特殊ルールが正しく機能
- UI/UXが快適

**完了条件**:
- 5ゲーム連続完走成功
- 新規バグが発見されない

**依存**: Task 7.1, Task 7.2

**推定時間**: 1時間

---

### Task 7.4: ドキュメント更新

**実装内容**:
- README.md 更新（Phase 1.5の機能を追記）
- CHANGELOG.md 作成（Phase 1.5の変更履歴）
- コーディング規約準拠の確認

**完了条件**:
- ドキュメントが最新
- コーディング規約に準拠

**依存**: Task 7.3

**推定時間**: 30分

---

## 完了基準

以下の全てが満たされたら Phase 1.5 完了：

1. ✅ 全タスクが完了
2. ✅ 全テストがパス（Phase 1 + Phase 1.5）
3. ✅ ゲーム開始から終了まで エラーなく完走
4. ✅ 全パターン（単体、ペア、3枚、4枚、階段）が動作
5. ✅ 革命システムが正しく機能
6. ✅ 特殊ルール（8切り、縛り、11バック、禁止上がり）が複数枚対応
7. ✅ CPUが複数枚を適切にプレイ
8. ✅ UIが分かりやすい
9. ✅ コーディング規約準拠
10. ✅ 5ゲーム連続完走成功

---

## 総タスク数と推定時間

**総タスク数**: 28タスク

**推定時間**:
- フェーズ1: 3-5時間
- フェーズ2: 4-6時間
- フェーズ3: 3.5-4.5時間
- フェーズ4: 3-4時間
- フェーズ5: 4-5時間
- フェーズ6: 7-11時間
- フェーズ7: 3.5-4時間

**合計**: 28-39.5時間（約4-5日、1日8時間作業として）

---

## 次のステップ

この実装タスクリストを基に、以下を実行する：

- **/implement**: タスクを順次実装（TDD推奨）

# Phase 1: 実装タスク

## 目的

phase1-plan.mdに基づき、実装を具体的なタスクに分解する。

各タスクは以下を満たす：
- **独立性**: 他のタスクに依存せず実装可能
- **テスト可能性**: 完了を確認できる
- **明確な完了条件**: Done/Not Doneが明確

---

## フェーズ1: データ層

### Task 1.1: CardSO スクリプト作成

**ファイル**: `Assets/_Project/Scripts/Data/CardSO.cs`

**実装内容**:
- Suitのenum定義（Spade, Heart, Diamond, Club）
- SerializeFieldプロパティ：cardSuit, rank, cardSprite
- GetStrength()メソッド実装

**完了条件**:
- CardSO.csが作成され、コンパイルエラーなし
- GetStrength()が正しい値を返す（2=15, A=14, 3-K=rank）

---

### Task 1.2: DeckSO スクリプト作成

**ファイル**: `Assets/_Project/Scripts/Data/DeckSO.cs`

**実装内容**:
- List<CardSO> allCards（Inspector設定用）
- List<CardSO> currentDeck（ランタイム用）
- Initialize()メソッド
- Shuffle()メソッド（Fisher-Yates）
- DrawCard()メソッド
- DistributeCards(PlayerHandSO[])メソッド

**完了条件**:
- DeckSO.csが作成され、コンパイルエラーなし
- Shuffle()が正しくシャッフルする
- DistributeCards()が均等に配布する

---

### Task 1.3: PlayerHandSO スクリプト作成

**ファイル**: `Assets/_Project/Scripts/Data/PlayerHandSO.cs`

**実装内容**:
- int playerID（Inspector設定）
- List<CardSO> cardsInHand（ランタイム）
- Initialize()メソッド
- AddCard(CardSO)メソッド
- RemoveCard(CardSO)メソッド
- SortByStrength()メソッド
- GetPlayableCards(int fieldStrength)メソッド
- プロパティ：Cards, CardCount, IsEmpty

**完了条件**:
- PlayerHandSO.csが作成され、コンパイルエラーなし
- AddCard/RemoveCardが正常動作
- SortByStrength()が強さ昇順にソート
- GetPlayableCards()が正しくフィルタリング

---

### Task 1.4: CardSO アセット作成（52枚）

**配置**: `Assets/_Project/ScriptableObjects/Cards/`

**実装内容**:
1. ディレクトリ作成：Spades/, Hearts/, Diamonds/, Clubs/
2. 各スート13枚 × 4スート = 52枚のアセット作成
3. 各アセットにSprite割り当て（Kenney assets）

**完了条件**:
- 52枚のCardSOアセットが作成されている
- 各アセットが正しいSprite, Suit, Rankを持つ

---

### Task 1.5: DeckSO & PlayerHandSO アセット作成

**配置**: `Assets/_Project/ScriptableObjects/Data/`

**実装内容**:
1. Deck.asset作成、allCards に52枚設定
2. PlayerHand_0.asset（playerID=0）作成
3. PlayerHand_1.asset（playerID=1）作成
4. PlayerHand_2.asset（playerID=2）作成
5. PlayerHand_3.asset（playerID=3）作成

**完了条件**:
- 5つのアセットが作成され、正しく設定されている

---

## フェーズ2: EventChannel層

### Task 2.1: CardEventChannelSO スクリプト作成

**ファイル**: `Assets/_Project/Scripts/Events/CardEventChannelSO.cs`

**実装内容**:
- GenericEventChannelSO<CardSO>を継承
- CreateAssetMenu属性設定

**完了条件**:
- CardEventChannelSO.csが作成され、コンパイルエラーなし

---

### Task 2.2: EventChannel & Variables アセット作成

**配置**: `Assets/_Project/ScriptableObjects/EventChannels/` & `Variables/`

**実装内容**:
1. OnGameStarted.asset（VoidEventChannelSO）
2. OnTurnChanged.asset（IntEventChannelSO）
3. OnCardPlayed.asset（CardEventChannelSO）
4. OnPassButtonClicked.asset（VoidEventChannelSO）
5. OnFieldReset.asset（VoidEventChannelSO）
6. OnGameEnded.asset（IntEventChannelSO）
7. currentPlayerTurn.asset（IntVariableSO）
8. isGameActive.asset（BoolVariableSO）

**完了条件**:
- 8つのアセットが作成されている

---

## フェーズ3: ゲームロジック層

### Task 3.1: RuleValidator スクリプト作成

**ファイル**: `Assets/_Project/Scripts/Core/RuleValidator.cs`

**実装内容**:
- CanPlayCard(CardSO, CardSO)メソッド
- IsCardInHand(CardSO, PlayerHandSO)メソッド

**完了条件**:
- RuleValidator.csが作成され、コンパイルエラーなし
- CanPlayCard()が正しくルール判定

---

### Task 3.2: TurnManager スクリプト作成

**ファイル**: `Assets/_Project/Scripts/Core/TurnManager.cs`

**実装内容**:
- SerializeField：currentPlayerTurn, onTurnChanged, onFieldReset
- int consecutivePassCount
- Initialize()メソッド
- NextTurn()メソッド
- OnCardPlayed()メソッド
- OnPlayerPass()メソッド
- ResetField()メソッド

**完了条件**:
- TurnManager.csが作成され、コンパイルエラーなし
- NextTurn()が正しくターン進行
- ResetField()が3人パス後に発火

---

### Task 3.3: GameManager スクリプト作成

**ファイル**: `Assets/_Project/Scripts/Core/GameManager.cs`

**実装内容**:
- SerializeField：deck, playerHands[], EventChannels, turnManager, ruleValidator
- CardSO currentFieldCard
- StartGame()メソッド
- HandleCardPlayed(CardSO)メソッド
- HandlePassButtonClicked()メソッド
- HandleFieldReset()メソッド
- EndGame(int)メソッド
- OnEnable/OnDisable でEventChannel購読

**完了条件**:
- GameManager.csが作成され、コンパイルエラーなし
- StartGame()がカード配布・ソート・イベント発火
- HandleCardPlayed()がルール検証・手札更新・勝利判定・ターン進行
- HandleFieldReset()が currentFieldCard をクリア

---

### Task 3.4: AIPlayer スクリプト作成

**ファイル**: `Assets/_Project/Scripts/Core/AIPlayer.cs`

**実装内容**:
- SerializeField：ruleValidator
- DecideAction(PlayerHandSO, CardSO)メソッド
  - プレイ可能カード抽出
  - 最弱カード選択

**完了条件**:
- AIPlayer.csが作成され、コンパイルエラーなし
- DecideAction()が正しく最弱カードを返す

---

### Task 3.5: AIController スクリプト作成

**ファイル**: `Assets/_Project/Scripts/Core/AIController.cs`

**実装内容**:
- SerializeField：aiTurnDelay, playerHands[], aiPlayer, gameManager, EventChannels
- OnEnable/OnDisableでonTurnChanged購読
- HandleTurnChanged(int)メソッド
- ExecuteAITurn(int)コルーチン
  - 1.5秒待機
  - aiPlayer.DecideAction()
  - カードプレイ or パス

**完了条件**:
- AIController.csが作成され、コンパイルエラーなし
- CPUターンで自動的にカードプレイ/パス

---

## フェーズ4: UI層

### Task 4.1: UXML/USS ファイル作成

**ファイル**:
- `Assets/_Project/UI/UXML/GameScreen.uxml`
- `Assets/_Project/UI/USS/Common.uss`
- `Assets/_Project/UI/USS/GameScreen.uss`
- `Assets/_Project/UI/USS/Card.uss`

**実装内容**:
- Common.uss：デザイントークン定義
- GameScreen.uss：レイアウトスタイル
- Card.uss：カードスタイル（.card, .card--selected, .card--playable）
- GameScreen.uxml：UI構造定義

**完了条件**:
- 4つのファイルが作成されている
- UXMLがUSSを正しく参照

---

### Task 4.2: CardUI スクリプト作成

**ファイル**: `Assets/_Project/Scripts/UI/2D/CardUI.cs`

**実装内容**:
- コンストラクタ（CardSO）
- VisualElement生成
- SetSelected(bool)メソッド
- AddClass/RemoveClassメソッド
- プロパティ：Element, CardData

**完了条件**:
- CardUI.csが作成され、コンパイルエラーなし
- CardUIが正しくカード画像を表示

---

### Task 4.3: HandUI スクリプト作成

**ファイル**: `Assets/_Project/Scripts/UI/2D/HandUI.cs`

**実装内容**:
- コンストラクタ（VisualElement, PlayerHandSO, CardEventChannelSO）
- Refresh()メソッド（手札を再描画）
- OnCardClicked(CardUI)メソッド（1枚選択ロジック）
- GetSelectedCards()メソッド
- ClearSelection()メソッド
- HighlightPlayableCards(List<CardSO>)メソッド
- event Action OnSelectionChanged

**完了条件**:
- HandUI.csが作成され、コンパイルエラーなし
- カードクリックで選択状態変更
- OnSelectionChanged イベント発火

---

### Task 4.4: GameScreenUI スクリプト作成（基本機能）

**ファイル**: `Assets/_Project/Scripts/UI/2D/GameScreenUI.cs`

**実装内容**:
- SerializeField：playerHands[], EventChannels
- ランタイム状態：currentPlayerID, isGameActive
- OnEnable/OnDisableでEventChannel購読
- HandleGameStarted()メソッド（HandUI初期化）
- HandleTurnChanged(int)メソッド（ターン表示更新）
- HandleCardPlayed(CardSO)メソッド（手札リフレッシュ、場表示）
- HandleFieldReset()メソッド（場クリア）
- HandleGameEnded(int)メソッド（終了画面表示）
- OnPlayCardButtonClick()メソッド
- OnPassButtonClick()メソッド

**完了条件**:
- GameScreenUI.csが作成され、コンパイルエラーなし
- ボタンクリックで正しくイベント発火
- ターン表示が更新される

---

### Task 4.5: ボタン状態管理実装

**ファイル**: `Assets/_Project/Scripts/UI/2D/GameScreenUI.cs`（追加実装）

**実装内容**:
- UpdateButtonStates()メソッド
  - Play Cardボタン：currentPlayerID==0 AND isGameActive AND selectedCards.Count > 0
  - Passボタン：currentPlayerID==0 AND isGameActive
- HandUI.OnSelectionChanged購読で UpdateButtonStates()呼び出し

**完了条件**:
- ボタンが正しく有効/無効切り替わる

---

### Task 4.6: プレイ可能カードハイライト実装

**ファイル**: `Assets/_Project/Scripts/UI/2D/GameScreenUI.cs`（追加実装）

**実装内容**:
- UpdatePlayableCardsHighlight()メソッド
  - currentPlayerID != 0 → ハイライトなし
  - currentPlayerID == 0 → playerHands[0].GetPlayableCards()
  - HandUI.HighlightPlayableCards()呼び出し
- HandleTurnChanged()とHandleFieldReset()から呼び出し

**完了条件**:
- プレイ可能カードに緑枠が表示される
- CPUターンではハイライトなし

---

### Task 4.7: ゲーム終了画面実装

**ファイル**: `Assets/_Project/Scripts/UI/2D/GameScreenUI.cs`（追加実装）

**実装内容**:
- ShowGameEndScreen(int winnerID)メソッド
  - winnerID==0 → "You Win!"
  - winnerID!=0 → "CPU X Wins!"
  - game-end-screen--hiddenクラス削除
- OnRestartButtonClick()メソッド
  - SceneManager.LoadScene()

**完了条件**:
- 勝利時に終了画面表示
- Restartボタンでゲーム再開

---

## フェーズ5: 統合とテスト

### Task 5.1: シーンセットアップ

**シーン**: `Assets/_Project/Scenes/GameScene.unity`

**実装内容**:
1. 空のGameObjectに GameManager アタッチ
2. GameObjectに TurnManager, RuleValidator, AIPlayer, AIController アタッチ
3. UIDocument を持つGameObject作成、GameScreenUI アタッチ
4. GameScreen.uxml を UIDocument に設定
5. Inspector でSerializeField を全て設定

**完了条件**:
- シーンが正しくセットアップされている
- Inspector設定が全て完了

---

### Task 5.2: プレイテスト（手動）

**テストシナリオ**:
1. ゲーム開始確認
2. 手札表示確認
3. カード選択確認
4. カードプレイ確認
5. パス確認
6. CPUターン確認
7. 場の流れ確認
8. 勝利確認

**完了条件**:
- 全シナリオがエラーなく完走

---

### Task 5.3: バグ修正

**実装内容**:
プレイテストで発見されたバグを修正

**完了条件**:
- クリティカルバグが全て修正されている

---

## フェーズ6: 最終調整

### Task 6.1: UI調整

**実装内容**:
- カードサイズ調整
- レイアウト調整
- 色調整

**完了条件**:
- UIが見やすく、操作しやすい

---

### Task 6.2: 最終プレイテスト

**テストシナリオ**:
- 連続5ゲーム完走
- 全機能動作確認

**完了条件**:
- 5ゲーム連続完走成功

---

## 完了基準

以下の全てが満たされたら Phase 1 完了：

1. ✅ 全タスクが完了
2. ✅ ゲーム開始から終了まで エラーなく完走
3. ✅ ルールが正しく機能
4. ✅ CPUが適切にプレイ
5. ✅ UIが分かりやすい
6. ✅ コーディング規約準拠

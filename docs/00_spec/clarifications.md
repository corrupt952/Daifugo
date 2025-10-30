# Phase 1: 設計判断と明確化

## 目的

phase1-spec.mdで定義した仕様の曖昧な部分を明確化し、設計判断の理由を記録する。

---

## C-001: 開始プレイヤーの決定方法

### 質問
ゲーム開始時、誰から始めるか？

### 選択肢
1. **3♠を持つプレイヤー**（大富豪の一般的なルール）
2. **プレイヤー0（人間）固定**
3. **ランダム**

### 判断
**プレイヤー0（人間）固定**

### 理由
- Phase 1は学習目的のため、シンプルさを優先
- 常に人間から開始することで、ゲーム開始時の動作確認が容易
- 3♠検索のロジックが不要で実装が簡単

---

## C-002: CPUの思考時間

### 質問
CPUがカードをプレイするまでの待機時間は？

### 選択肢
1. **即座（0秒）**
2. **1秒**
3. **1.5秒**
4. **ランダム（1〜2秒）**

### 判断
**1.5秒固定**

### 理由
- プレイヤーがCPUのアクションを視認できる時間が必要
- 1.5秒は「考えている」感があり、自然な間
- 固定値にすることで実装とテストが容易

---

## C-003: カード選択のUI動作

### 質問
Phase 1では1枚出しのみだが、UI上で複数枚選択可能にするか？

### 選択肢
1. **1枚のみ選択可能**（選択すると前の選択は解除）
2. **複数枚選択可能**（Phase 2への拡張を見据える）

### 判断
**1枚のみ選択可能**

### 理由
- Phase 1のスコープは1枚出しのみ
- YAGNI原則（You Aren't Gonna Need It）に従う
- シンプルな実装で動作を確実にする
- Phase 2で必要になったら拡張する

---

## C-004: 場の流れの判定方法

### 質問
「カードを出したプレイヤー以外の全員がパス」をどう判定するか？

### 大富豪の正確なルール
カードを出したプレイヤー（親）以外の全員がパスし、親に手番が戻ってきた時に場を流す。

### 実装方針
- **親の追跡**方式を使用
- `lastCardPlayerID`を記録（カードを出した人）
- `passedPlayers`をSet<int>で記録（パスした人）
- カードがプレイされたら：
  - `lastCardPlayerID = currentPlayer`
  - `passedPlayers.Clear()`
- パスされたら：
  - `passedPlayers.Add(currentPlayer)`
- ターン開始時に判定：
  - `currentPlayer == lastCardPlayerID` かつ
  - `passedPlayers.Count == 3`（親以外の全員）
  - → 場をリセット

### 注意点
- 4人プレイ固定のため、親以外は3人
- ゲーム開始時（lastCardPlayerID == -1）は場リセット判定しない

---

## C-005: ゲーム再開の方法

### 質問
「Restart」ボタンでどのようにゲームを再開するか？

### 選択肢
1. **シーンリロード**（`SceneManager.LoadScene`）
2. **GameManagerのReset()メソッド**
3. **新しいシーンへ遷移**

### 判断
**シーンリロード**

### 理由
- Phase 1では最もシンプルな方法
- ScriptableObjectの状態リセットが自動的に行われる
- 実装が最小限で済む
- Phase 2以降で適切なリセットロジックを実装可能

---

## C-006: プレイ可能カードの判定とハイライト

### 質問
どのタイミングでプレイ可能カードをハイライトするか？

### 実装方針
- **ターン開始時**
- **場の状態変更時**（カードプレイ、場の流れ）

### ハイライト方法
- USSクラス`.card--playable`を付与
- 緑色の枠線で視覚的に表現

### 判定ロジック
```
IF 場が空:
    全カードがプレイ可能
ELSE:
    場のカード強さ > 自分のカード強さ のカードのみプレイ可能
```

### 人間ターン以外
- CPU ターン中はハイライトなし（人間は操作不可）

---

## C-007: ターン進行のタイミング

### 質問
カードプレイ/パス後、いつ次のターンに進むか？

### 実装方針
1. **人間プレイヤー**: カードプレイ/パス確定後、即座にターン進行
2. **CPUプレイヤー**: アクション実行後、即座にターン進行

### イベントフロー
```
カードプレイ/パス
  ↓
ゲームロジック処理（手札更新、場更新）
  ↓
勝利判定
  ↓ 勝利していない場合
ターン進行
  ↓
OnTurnChanged イベント発火
  ↓
UI更新 / CPU処理開始
```

---

## C-008: イベント駆動アーキテクチャ

### 質問
EventChannelとVariablesをどう使い分けるか？

### 設計方針

#### EventChannel（イベント通知）
以下のイベントにEventChannelを使用：

**コマンドイベント**（UI/AIからGameManagerへのリクエスト）:
- **OnPlayCardRequested**（カードプレイリクエスト、CardSO: card）
- **OnPassButtonClicked**（パスボタンクリック）

**通知イベント**（GameManagerから全コンポーネントへの通知）:
- **OnGameStarted**（ゲーム開始）
- **OnTurnChanged**（ターン変更、int: playerID）
- **OnCardPlayed**（カードプレイ完了通知、CardPlayedEventData: card, playerID, fieldCard）
- **OnFieldReset**（場のリセット）
- **OnGameEnded**（ゲーム終了、int: winnerID）

#### Variables（状態管理）
Phase 1では**Variablesを使用しない**。

理由：
- 全ての状態はイベント経由で通知
- 各コンポーネントはローカル変数で状態を保持
- 完全な疎結合を実現

#### 状態管理の方針
- **ゲーム進行フラグ**: GameManagerがローカル変数で管理、OnGameStarted/OnGameEndedで通知
- **現在のプレイヤー**: TurnManagerがローカル変数で管理、OnTurnChangedで通知
- **場のカード**: GameManagerがローカル変数で管理、OnCardPlayed時にイベントに含める

#### Subscriber側の原則
- 全コンポーネント：EventChannelのみを購読
- イベント受信時にローカル変数で状態を保持
- コンポーネント間の直接参照を避ける

### イベントの分離とOnCardPlayedイベントの拡張

**コマンドと通知の分離**:
カードプレイのリクエスト（コマンド）と完了通知を分離：
- **OnPlayCardRequested(CardSO card)** - UI/AIがカードをプレイしたい時に発火
- **OnCardPlayed(CardPlayedEventData)** - GameManagerがカードプレイ処理完了後に発火

**OnCardPlayed通知イベントの構造**:
```csharp
public struct CardPlayedEventData
{
    public CardSO Card;        // プレイされたカード
    public int PlayerID;       // プレイしたプレイヤー
    public CardSO FieldCard;   // プレイ後の場のカード
}
```

これにより：
- AIControllerはOnCardPlayedを購読して currentFieldCard をローカルで追跡
- GameManagerへの直接依存が完全に排除される
- イベント駆動アーキテクチャが完全に実現される

### 理由
- 完全な疎結合を実現
- Inspector設定不要で動作
- テストとデバッグが容易
- コンポーネント間の依存を排除

---

## C-009: CPU決定ロジック

### 質問
CPUはどのようにカードを選択するか？

### 実装方針
**最弱プレイ可能カード戦略**

### アルゴリズム
```
1. 手札から場のカードより強いカードを抽出
2. プレイ可能カードがない場合 → パス
3. プレイ可能カードがある場合:
   a. 強さでソート（昇順）
   b. 最弱のカードを選択
   c. そのカードをプレイ
```

### 理由
- シンプルで実装が容易
- 強いカードを温存する基本戦略
- Phase 1の目的（ゲームとして成立）を満たす

---

## C-010: 手札のソート順

### 質問
手札をどの順序で表示するか？

### 選択肢
1. **強さの昇順**（弱 → 強）
2. **強さの降順**（強 → 弱）
3. **ソートなし**

### 判断
**強さの昇順（弱 → 強）**

### 理由
- 左から右へ、弱いカードから強いカードの順
- プレイヤーが手札を把握しやすい
- 一般的なカードゲームの慣習に従う

---

## C-011: ボタンの有効/無効状態

### 質問
「Play Card」「Pass」ボタンの有効/無効をどう制御するか？

### 実装方針

#### Play Cardボタン
**有効条件**:
- `currentPlayerID == 0`（人間のターン）
- AND `isGameActive == true`（ゲーム進行中）
- AND `selectedCards.Count > 0`（カード選択済み）

**無効条件**:
- 上記以外

#### Passボタン
**有効条件**:
- `currentPlayerID == 0`（人間のターン）
- AND `isGameActive == true`（ゲーム進行中）

**無効条件**:
- 上記以外

### 更新タイミング
- ターン変更時（OnTurnChanged）
- カード選択状態変更時（HandUI.OnSelectionChanged）
- ゲーム終了時（OnGameEnded）

---

## C-012: カード画像アセット

### 質問
Kenney Playing Cards Packのどのサイズを使用するか？

### 選択肢
- Small（140x190）
- Medium（280x380）
- Large（420x570）

### 判断
**Medium（280x380）**

### 理由
- 1920x1080解像度で適切なサイズ
- 手札13枚を横並びで表示可能
- パフォーマンスとクオリティのバランス

---

## C-013: エラーハンドリング

### 質問
ルール違反（弱いカードをプレイしようとする）をどう処理するか？

### 実装方針
**二重チェック方式**

#### UI層（HandUI）
- プレイ可能カードのみ選択可能
- プレイ不可カードは選択できない（クリック無効）

#### ゲームロジック層（GameManager）
- カードプレイ時に再度ルール検証
- ルール違反の場合：
  - 警告ログ出力
  - カードプレイを却下
  - ターン進行せず、手番継続

### 理由
- UI層とロジック層で二重チェック
- 不正なプレイを確実に防止
- デバッグ時にログで確認可能

---

## C-014: プレイ可能カード判定ロジックの統合（リファクタリング）

### 背景
Phase 1初期実装では、プレイ可能カード判定ロジックが複数箇所に分散していた：
- `PlayerHandSO.GetPlayableCards()` - データ層がロジックを持つ（責任違反）
- `RuleValidator` - ルール検証のみを担当
- `PlayableCardService` - ターン判定とロジックが混在
- `IRuleValidator` インターフェース - 実装が1つのみ（過度な抽象化）

### 問題点
1. **関心の分離違反**: PlayerHandSOというデータオブジェクトがロジックを持っている
2. **重複した判定**: 複数箇所で同様のロジックが実装される可能性
3. **テストの複雑さ**: MockRuleValidatorが必要で、テストが冗長
4. **Phase 2への拡張性**: 革命・縛りなどの複雑なルール追加時、修正箇所が分散

### リファクタリング方針
**単一の純粋C#クラスに統合**

#### 新しいアーキテクチャ
```
FieldState (struct)
  ↓ データ
PlayableCardsCalculator (純粋クラス)
  ↓ ルール設定
GameRulesSO (ScriptableObject)
```

#### 実装詳細

**FieldState（データ構造）**:
```csharp
public struct FieldState
{
    public bool IsEmpty { get; }
    public CardSO CurrentCard { get; }
    public int Strength { get; }

    public static FieldState Empty();
    public static FieldState FromCard(CardSO card);
}
```
- 場の状態を表す軽量なデータ構造
- Phase 2で革命状態などを追加予定

**GameRulesSO（ルール設定）**:
```csharp
[CreateAssetMenu(...)]
public class GameRulesSO : ScriptableObject
{
    [SerializeField] private bool enableRevolution = false;  // Phase 2
    [SerializeField] private bool enable8Cut = true;         // Phase 1
    [SerializeField] private bool enableBind = false;        // Phase 2
    [SerializeField] private bool enableSpade3Return = false; // Phase 2
}
```
- どのルールが有効/無効かを設定
- Phase 1では8-cutのみ実装、他はPhase 2用に準備

**PlayableCardsCalculator（ロジック）**:
```csharp
public class PlayableCardsCalculator
{
    public List<CardSO> GetPlayableCards(PlayerHandSO hand, FieldState fieldState, GameRulesSO gameRules);
    public bool CanPlayCard(CardSO card, FieldState fieldState, GameRulesSO gameRules);
    public bool IsCardInHand(CardSO card, PlayerHandSO hand);
}
```
- 純粋C#クラス（MonoBehaviourでもScriptableObjectでもない）
- 副作用なし、テストが容易
- Constructor Injectionで GameRulesSO を受け取る

#### 依存性注入パターン
```csharp
// GameManager.cs
private GameLogic gameLogic;
[SerializeField] private GameRulesSO gameRules;

private void Awake()
{
    gameLogic = new GameLogic(gameRules);
}

// GameLogic.cs
public class GameLogic
{
    private readonly PlayableCardsCalculator calculator;
    private readonly GameRulesSO gameRules;

    public GameLogic(GameRulesSO gameRules)
    {
        calculator = new PlayableCardsCalculator();
        this.gameRules = gameRules;
    }
}
```

### 削除したコンポーネント
- `PlayerHandSO.GetPlayableCards()` メソッド
- `RuleValidator.cs`
- `PlayableCardService.cs`
- `IRuleValidator` インターフェース
- `MockRuleValidator.cs`
- `RuleValidatorTests.cs`
- `PlayableCardServiceTests.cs`

### 修正したコンポーネント
- `GameLogic` - PlayableCardsCalculator使用、GameRulesSOを注入
- `GameManager` - GameRulesSOをSerializeFieldで保持
- `AIPlayerStrategy` - PlayableCardsCalculator使用、GameRulesSOを注入
- `AIController` - GameRulesSOをSerializeFieldで保持
- `GameScreenUI` - PlayableCardsCalculator使用、ターン判定をUI層に移動
- `GameLogicTests` - MockRuleValidator削除、GameRulesSOを直接使用
- `AIPlayerStrategyTests` - GameRulesSOを注入

### 新規作成したコンポーネント
- `FieldState.cs` - 場の状態データ構造
- `GameRulesSO.cs` - ルール設定ScriptableObject
- `PlayableCardsCalculator.cs` - プレイ可能カード判定ロジック
- `PlayableCardsCalculatorTests.cs` - Edit Modeテスト（17テストケース）

### 利点
1. **単一責任**: データ・ルール・ロジックが明確に分離
2. **テスト容易性**: 純粋関数、Edit Modeで高速テスト、モック不要
3. **Phase 2拡張性**: FieldStateとGameRulesSOに拡張ポイントが明確
4. **YAGNI準拠**: 不要な抽象化（IRuleValidator）を削除
5. **Functional Core**: ロジックがUnityから完全に独立

### Phase 2への拡張例
```csharp
// 革命ルールを追加する場合
public struct FieldState
{
    public bool IsRevolutionActive { get; set; }  // 追加
}

public bool CanPlayCard(CardSO card, FieldState fieldState, GameRulesSO gameRules)
{
    if (fieldState.IsEmpty) return true;

    // 革命時は判定反転
    if (gameRules.IsRevolutionEnabled && fieldState.IsRevolutionActive)
    {
        return card.GetStrength() < fieldState.Strength;
    }

    return card.GetStrength() > fieldState.Strength;
}
```

### 理由
- アーキテクチャの質的向上
- 純粋関数的アプローチによるバグ削減
- 将来の拡張を見越した最小限の実装
- コーディング規約への完全準拠

---

## まとめ

これらの設計判断により、Phase 1の仕様が明確になった。

次のステップ：
- **/plan**: 技術実装計画の策定
- **/tasks**: 実装タスクへの分解

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

# Phase 1.5: 複数枚出し機能 - 設計判断と明確化

## C-015: プレイパターンのデータ構造

### 質問
複数枚のプレイパターン（単体/ペア/3枚/4枚/階段）をどのデータ構造で表現するか？

### 選択肢
1. **enum PlayPattern**（単純な列挙）
2. **struct PlayPattern**（枚数やカード情報を含む）
3. **class PlayPattern**（メソッドを含む）

### 判断
**enum PlayPattern**

```csharp
public enum PlayPattern
{
    Single,      // 1枚
    Pair,        // 2枚
    Triple,      // 3枚
    Quadruple,   // 4枚
    Sequence     // 階段（3枚以上）
}
```

### 理由
- Phase 1.5では枚数が固定（ペア=2枚、3枚=3枚など）
- シンプルなenumで十分
- パターン判定ロジックは `PlayPatternDetector` クラスに分離
- Phase 2以降で複雑化したら struct に変更可能

---

## C-016: 階段の強さ比較方法

### 質問
階段同士を比較する時、どのカードで強さを判定するか？

### 選択肢
1. **最も強いカード**で比較（例: ♠3-4-5 vs ♠6-7-8 → 8 vs 5）
2. **最も弱いカード**で比較（例: ♠3-4-5 vs ♠6-7-8 → 6 vs 3）
3. **中央のカード**で比較

### 判断
**最も強いカード**で比較

### 理由
- 大富豪の一般的なルールに従う
- 直感的（階段の「頂点」で比較）
- 実装がシンプル（`cards.Max(c => c.GetStrength())`）

### 革命時の扱い
革命中は**最も弱いカード**で比較（強さ逆転のため）

---

## C-017: 革命と11バックの相互作用

### 質問
革命中に11バック（J）がプレイされた場合、どうなるか？

### 設計方針
**11バックは「現在の状態を反転」させる**

#### パターン1: 通常 → 11バック
- 通常状態（3=3, 2=15）
- Jプレイ → 一時革命発動
- 一時革命状態（3=15, 2=2）
- 場が流れる → 通常に戻る

#### パターン2: 革命 → 11バック
- 革命状態（3=15, 2=2）← 4枚出しで発動
- Jプレイ → 一時的に通常に戻る
- 一時通常状態（3=3, 2=15）
- 場が流れる → 革命に戻る

### 実装方針
```csharp
// FieldState
public readonly bool IsRevolutionActive;       // 永続革命（ゲーム終了まで）
public readonly bool IsTemporaryRevolution;    // 一時革命（場が流れるまで）

// 実効的な革命状態
bool effectiveRevolution = IsRevolutionActive XOR IsTemporaryRevolution;
```

### 理由
- XOR（排他的論理和）で自然に表現できる
- Phase 2以降の複雑なルール追加にも対応可能
- 状態管理がシンプル

---

## C-018: 縛りの詳細判定（複数枚）

### 質問
複数枚プレイ時の縛り判定はどうするか？

### 設計方針
**最後の2プレイが「同じスートで統一されている」場合のみ縛り発動**

#### ケース1: ペア → ペア
```
プレイ1: ♠5 ♠5（同じスート統一）
プレイ2: ♠7 ♠7（同じスート統一）
→ 縛り発動（スペード）
```

#### ケース2: 混合スート → 統一スート
```
プレイ1: ♠5 ♥5（混合スート）
プレイ2: ♠7 ♠7（同じスート統一）
→ 縛らない（プレイ1が混合）
```

#### ケース3: 階段 → 階段
```
プレイ1: ♠3-4-5（スペード階段）
プレイ2: ♠6-7-8（スペード階段）
→ 縛り発動（スペード）
```

### 判定アルゴリズム
```csharp
bool IsBindingActive(FieldState fieldState)
{
    if (!gameRules.IsBindEnabled) return false;
    if (fieldState.CardsInField.Count < 2) return false;

    // 最後のプレイのカードを取得
    var lastPlay = GetLastPlay(fieldState);
    var secondLastPlay = GetSecondLastPlay(fieldState);

    // 両方とも同じスートで統一されているか
    bool lastPlayUnified = IsAllSameSuit(lastPlay) && !ContainsJoker(lastPlay);
    bool secondLastPlayUnified = IsAllSameSuit(secondLastPlay) && !ContainsJoker(secondLastPlay);

    // 両方とも統一 かつ 同じスート
    if (lastPlayUnified && secondLastPlayUnified)
    {
        return lastPlay[0].CardSuit == secondLastPlay[0].CardSuit;
    }

    return false;
}
```

### 理由
- 大富豪の一般的な縛りルールに従う
- ジョーカーは縛りに影響しない
- 混合スートのペアは縛りの対象外

---

## C-019: AI戦略の詳細（複数枚出し）

### 質問
AIは複数枚をどう判断するか？特に革命を起こすべきか？

### 設計方針
**Phase 1.5では「革命を起こさない」シンプル戦略**

#### 基本方針
1. 場のパターンに合うカードを抽出
2. プレイ可能な最弱の組み合わせを選択
3. 4枚持っていても、3枚で出す（革命回避）

#### 具体例
```
場が空の時:
  - 単体で最弱カードを出す（ペア温存）
  - 理由: ペアは「強い札を確実に出せる手段」として温存

場がペアの時:
  - 手持ちのペアから最弱を出す
  - 4枚持っていても2枚だけ出す

場が3枚の時:
  - 手持ちの3枚から最弱を出す

場が階段3枚の時:
  - 手持ちの階段3枚から最弱を出す
```

### なぜ革命を起こさないか
1. **手札評価が複雑**: 革命が有利かどうかは手札全体を見ないと判断できない
2. **Phase 1.5のスコープ外**: 高度なAI戦略はPhase 2以降で実装
3. **シンプルさ優先**: 基本動作の確実性を重視

### Phase 2以降で実装予定
- 手札の平均強度を計算
- 革命が有利かどうかを判定
- 4枚を戦略的にプレイ

### 理由
- YAGNI原則（必要になってから実装）
- Phase 1.5の目的は「複数枚出しの基本実装」
- テストとデバッグを容易にする

---

## C-020: 階段の定義と制約

### 質問
階段の定義を明確にする。回り階段（K-A-2）やジョーカーを含む階段は？

### 設計方針

#### Phase 1.5で実装する階段
- **連続するランク**: 3-4-5、10-J-Q-K など
- **同じスート必須**: 全て♠、全て♥ など
- **最小3枚**: 2枚では階段にならない
- **最大13枚**: A-2-3-...-K-A（理論上）

#### Phase 1.5で実装しない階段
- **回り階段**: K-A-2、Q-K-A-2 など（Aを跨ぐ）
- **ジョーカーを含む階段**: ♠3-Joker-♠5（ジョーカーが4の代わり）
- **混合スート階段**: ♠3-♥4-♦5（縛りなし階段）

### 判定アルゴリズム
```csharp
bool IsSequence(List<CardSO> cards)
{
    if (cards.Count < 3) return false;

    // ジョーカーを含む場合は階段にならない（Phase 1.5）
    if (cards.Any(c => c.IsJoker)) return false;

    // 同じスートか確認
    var suit = cards[0].CardSuit;
    if (cards.Any(c => c.CardSuit != suit)) return false;

    // ランク順にソート
    var sorted = cards.OrderBy(c => c.Rank).ToList();

    // 連続しているか確認
    for (int i = 1; i < sorted.Count; i++)
    {
        if (sorted[i].Rank != sorted[i-1].Rank + 1)
            return false;
    }

    return true;
}
```

### Phase 2以降で拡張可能
- ジョーカーをワイルドカードとして使用
- 回り階段の対応（設定で有効/無効）

### 理由
- シンプルな実装でバグを減らす
- 大富豪の基本ルールに従う
- Phase 2での拡張ポイントを明確にする

---

## C-021: 禁止上がりの複数枚判定

### 質問
禁止カード（2、8、ジョーカー、スペ3）を含む複数枚で上がった場合、負けか？

### 設計方針
**1枚でも禁止カードが含まれていたら負け**

#### 判定例
```
ケース1: ♦2 ♣2で上がる → 負け（2を含む）
ケース2: ♦8 ♣8 ♥8で上がる → 負け（8を含む）
ケース3: ♠3 ♠4 ♠5の階段で上がる → 負け（スペ3を含む）
ケース4: Joker ♦5のペア → 不可能（Phase 1.5ではJokerは階段不可）
ケース5: ♦5 ♣5で上がる → 勝ち（禁止カードなし）
```

### 実装方針
```csharp
bool IsForbiddenFinish(List<CardSO> cards)
{
    if (!gameRules.IsForbiddenFinishEnabled) return false;

    foreach (var card in cards)
    {
        if (card.IsJoker) return true;
        if (card.Rank == 2) return true;
        if (card.Rank == 8) return true;
        if (card.CardSuit == CardSO.Suit.Spade && card.Rank == 3) return true;
    }

    return false;
}
```

### 理由
- 複数枚でも禁止上がりルールは適用
- 1枚でも含まれていたらアウト
- 大富豪の一般的なローカルルールに従う

---

## C-022: 複数枚選択UIの挙動

### 質問
複数枚選択時のUI挙動を明確にする。インテリジェントハイライトは実装するか？

### 設計方針
**Phase 1と同じ自由選択方式を維持**

#### 実装する機能
- 複数枚のクリック選択/解除
- 選択中のカードを `card--selected` クラスで視覚化
- 「Play Card」ボタンクリック時にバリデーション
- 無効な組み合わせは警告表示

#### 実装しない機能（Phase 2以降）
- インテリジェントハイライト（有効な次の手の提案）
- 自動組み合わせ提案
- 選択途中でのリアルタイムバリデーション

### UI操作フロー
```
1. プレイヤーがカードをクリック → 選択状態
2. さらにカードをクリック → 複数選択
3. 選択済みカードを再クリック → 選択解除
4. 「Play Card」ボタンをクリック
5. システムが組み合わせを検証
   - OK → カードプレイ
   - NG → エラー警告表示、選択状態は維持
```

### エラーメッセージ例
```
"無効な組み合わせです。同じランクまたは階段を選んでください"
"場と同じ枚数を出してください（2枚）"
"場と同じパターンを出してください（ペア）"
"より強いカードを出してください"
"縛り中です。♠のカードのみ出せます"
```

### 理由
- Phase 1の実装を最大限活用
- 実装コストを抑える
- ユーザーは試行錯誤できる（学習効果）
- インテリジェントハイライトはPhase 2で検討

---

## C-023: プレイパターン判定の優先順位

### 質問
選択されたカードから、どの順序でパターンを判定するか？

### 設計方針
**同じランク優先、次に階段**

#### 判定アルゴリズム
```csharp
PlayPattern DetectPattern(List<CardSO> cards)
{
    // 1. 単体
    if (cards.Count == 1) return PlayPattern.Single;

    // 2. 同じランク判定
    if (IsAllSameRank(cards))
    {
        if (cards.Count == 2) return PlayPattern.Pair;
        if (cards.Count == 3) return PlayPattern.Triple;
        if (cards.Count == 4) return PlayPattern.Quadruple;
    }

    // 3. 階段判定
    if (IsSequence(cards)) return PlayPattern.Sequence;

    // 4. どれでもない
    return PlayPattern.Invalid;
}
```

### 優先順位の理由
- **同じランクが優先**: 判定が高速（ソート不要）
- **階段は次**: ソートと連続性チェックが必要
- **4枚は特別扱い不要**: Quadrupleとして検出され、別途革命判定

### エッジケース
```
入力: ♠5 ♠5 ♠6 ♠7
  → 同じランクではない
  → 階段でもない（5が2枚ある）
  → Invalid
```

### 理由
- シンプルで予測可能
- パフォーマンス最適化
- バグを減らす

---

## C-024: 8切りの複数枚対応

### 質問
8のペア、8の3枚、8の4枚でも場をリセットするか？

### 設計方針
**8を含むプレイ全てで場をリセット**

#### 対象
- 8単体（Phase 1から実装済み）
- 8のペア（♦8 ♣8）
- 8の3枚（♦8 ♣8 ♥8）
- 8の4枚（♦8 ♣8 ♥8 ♠8）→ 革命も同時発動
- 8を含む階段（♠7-8-9）

### 実装方針
```csharp
bool ShouldResetField(List<CardSO> cards)
{
    if (!gameRules.Is8CutEnabled) return false;

    // 8を含むか
    return cards.Any(c => c.Rank == 8 && !c.IsJoker);
}
```

### 8の4枚の特殊ケース
```
8の4枚をプレイ:
  1. 場をリセット
  2. 革命発動
  3. 同じプレイヤー続行
  4. 革命状態で次のカードをプレイ
```

### 理由
- 8切りは「8があれば場をリセット」という単純なルール
- 複数枚でも一貫性を保つ
- 大富豪の一般的なローカルルールに従う

---

## C-025: データ構造の拡張方針

### 質問
Phase 1のデータ構造（FieldState、GameLogicなど）をどう拡張するか？

### 設計方針
**既存の構造を拡張、破壊的変更は避ける**

#### FieldState（struct）
```csharp
// Phase 1
public struct FieldState
{
    public readonly CardSO CurrentCard;  // 最後の1枚
    public bool IsEmpty => CurrentCard == null;
}

// Phase 1.5への拡張
public struct FieldState
{
    public readonly IReadOnlyList<CardSO> CardsInField;  // 全カード履歴
    public readonly bool IsRevolutionActive;              // 永続革命
    public readonly bool IsTemporaryRevolution;           // 一時革命

    public CardSO CurrentCard => CardsInField.LastOrDefault();
    public bool IsEmpty => CardsInField.Count == 0;

    // Factory Methods（イミュータブル）
    public static FieldState Empty();
    public static FieldState AddCard(FieldState current, CardSO card);
    public static FieldState AddCards(FieldState current, List<CardSO> cards);
}
```

#### GameLogic
```csharp
// Phase 1
CardPlayResult PlayCard(CardSO card, PlayerHandSO hand, FieldState fieldState);

// Phase 1.5への拡張（オーバーロード）
CardPlayResult PlayCards(List<CardSO> cards, PlayerHandSO hand, FieldState fieldState);
```

### 理由
- Phase 1のコードは動き続ける
- 段階的な移行が可能
- テストの追加が容易

---

## C-026: 革命中のカード強さ計算式

### 質問
革命が発動している時、カードの強さをどう計算するか？

### 背景
通常時のカード強さ:
- 3 = 3（最弱のランクカード）
- 4 = 4
- ...
- K(13) = 13
- A(1) = 14
- 2 = 15（最強のランクカード）
- Joker = 16（最強）

革命時は「弱いカードが強くなる」ため、3が最強、2が最弱になる必要がある。

### 設計方針
**革命時の強さマッピング**

```
Joker: 16（最強、変わらず）
3:     15（最強のランクカード）
4:     14
5:     13
6:     12
7:     11
8:     10
9:     9
10:    8
J(11): 7
Q(12): 6
K(13): 5
A(1):  3
2:     2（最弱のランクカード）
```

### 実装方針

```csharp
public int GetStrength(bool isRevolution = false)
{
    // Jokerは革命の影響を受けない
    if (IsJoker) return 16;

    if (isRevolution)
    {
        // 革命中の強さ計算
        if (Rank == 2) return 2;      // 2が最弱（Joker以外）
        if (Rank == 1) return 3;      // Aは3
        return 18 - Rank;             // 3 → 15, 4 → 14, ..., K(13) → 5
    }
    else
    {
        // 通常時の強さ計算（Phase 1から変更なし）
        if (Rank == 2) return 15;     // 2が最強（Joker以外）
        if (Rank == 1) return 14;     // Aは14
        return Rank;                   // 3-13はそのまま
    }
}
```

### 検証
```
通常時:
  3  → GetStrength(false) = 3
  A  → GetStrength(false) = 14
  2  → GetStrength(false) = 15
  Joker → GetStrength(false) = 16

革命時:
  3  → GetStrength(true) = 18 - 3 = 15  ✓（最強のランクカード）
  4  → GetStrength(true) = 18 - 4 = 14  ✓
  K  → GetStrength(true) = 18 - 13 = 5  ✓
  A  → GetStrength(true) = 3            ✓
  2  → GetStrength(true) = 2            ✓（最弱のランクカード）
  Joker → GetStrength(true) = 16       ✓（最強）
```

### 複数枚出しへの適用
階段の強さ比較時:
- **通常時**: 最も強いカードで比較（`cards.Max(c => c.GetStrength(false))`）
- **革命時**: 最も弱いカードで比較（`cards.Min(c => c.GetStrength(true))`）

例:
```
通常時: ♠3-4-5 vs ♠6-7-8
  → Max(3,4,5) = 5 vs Max(6,7,8) = 8
  → 5 < 8 → ♠6-7-8の勝ち

革命時: ♠3-4-5 vs ♠6-7-8（革命中の強さで計算）
  → Min(15,14,13) = 13 vs Min(12,11,10) = 10
  → 13 > 10 → ♠3-4-5の勝ち
```

### 11バックとの組み合わせ
実効的な革命状態 = `IsRevolutionActive XOR IsTemporaryRevolution`

強さ計算時:
```csharp
bool effectiveRevolution = fieldState.IsRevolutionActive ^ fieldState.IsTemporaryRevolution;
int strength = card.GetStrength(effectiveRevolution);
```

### 理由
- 数学的に一貫性がある（18 - Rank の公式）
- Jokerは常に最強（革命の影響を受けない）
- 2が最弱、3が最強（革命の定義に合致）
- 実装がシンプル

---

## まとめ

これらの設計判断により、Phase 1とPhase 1.5の仕様が明確になった。

次のステップ：
- **/plan**: 技術実装計画の策定
- **/tasks**: 実装タスクへの分解

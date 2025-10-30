# Daifugo コーディング規約

## 目的

本ドキュメントは、Daifugoプロジェクト（大富豪カードゲーム）における開発規約を定義します。

## 共通コーディング規約

**基本的なコーディング規約は、プロジェクト共通の規約に従ってください：**

📖 **[共通コーディング規約の全体像](coding_standards/README.md)** - ディレクトリ構造、ファイル一覧、プロジェクトタイプ別ガイド

### Core規約

- **[命名規則](coding_standards/core/naming-conventions.md)** - クラス、変数、メソッドの命名ルール
- **[コード構成](coding_standards/core/code-organization.md)** - ファイル構造、クラス設計
- **[コメント・ドキュメント](coding_standards/core/comments-documentation.md)** - XMLドキュメント、Tooltip
- **[エラーハンドリング](coding_standards/core/error-handling.md)** - null安全性、Try-Catch
- **[パフォーマンス](coding_standards/core/performance.md)** - Update最適化、キャッシング
- **[Unity固有](coding_standards/core/unity-specifics.md)** - Time.deltaTime、RequireComponent

### Architecture規約

- **[ScriptableObject](coding_standards/architecture/scriptableobject.md)** - データ管理パターン
- **[EventChannel](coding_standards/architecture/event-channels.md)** - イベント駆動通信
- **[Variables](coding_standards/architecture/variables.md)** - リアクティブ変数管理
- **[RuntimeSet](coding_standards/architecture/runtime-sets.md)** - 動的オブジェクト管理
- **[依存関係管理](coding_standards/architecture/dependency-management.md)** - 優先順位と実装パターン
- **[拡張パターン](coding_standards/architecture/extension-patterns.md)** - 新機能追加フロー

### UI規約

- **[BEM命名](coding_standards/ui/ui-toolkit/bem-naming.md)** - UI Toolkitクラス名規則
- **[デザイントークン](coding_standards/ui/ui-toolkit/design-tokens.md)** - USS変数活用
- **[UXML構造](coding_standards/ui/ui-toolkit/uxml-structure.md)** - UI構造設計
- **[レスポンシブ](coding_standards/ui/ui-toolkit/uss-responsive.md)** - 画面解像度対応
- **[World Space UI](coding_standards/ui/ugui/world-space-ui.md)** - 3D空間UIパターン
- **[Billboard](coding_standards/ui/ugui/billboard.md)** - カメラ追従UI
- **[uGUI Best Practices](coding_standards/ui/ugui/best-practices.md)** - uGUI実装規約

### コード例

- **[良い例: EventChannel](coding_standards/examples/good/event-channel-example.cs)**
- **[良い例: ScriptableObject](coding_standards/examples/good/scriptableobject-example.cs)**
- **[良い例: Variables](coding_standards/examples/good/variable-example.cs)**
- **[アンチパターン: Singleton乱用](coding_standards/examples/anti-patterns/singleton-abuse.cs)**
- **[アンチパターン: Update重い処理](coding_standards/examples/anti-patterns/update-heavy.cs)**

---

## Daifugo固有の規約

### プロジェクトの特性

- **シングルプレイヤー**: ネットワーク機能なし
- **2D → 3D段階開発**: Phase 1は2D（UI Toolkit）、Phase 2は3D（演出強化）
- **カードゲーム**: 大富豪のルール実装
- **学習プロジェクト**: UI Toolkit、ScriptableObject、EventChannelパターンの習得

### 技術スタック

#### Phase 1（2D版）
- Unity UI Toolkit 100%
- Tang3cko.EventChannels
- ScriptableObject
- Sprite画像

#### Phase 2（3D版）
- Unity 3D URP
- UI Toolkit（HUD部分）
- LitMotion（アニメーション）
- Cinemachine（カメラワーク）
- Particle System（演出）

---

## Namespace規約

### プロジェクト構造に基づくNamespace

```csharp
namespace Daifugo.Core      // ゲームコアロジック
namespace Daifugo.Data      // ScriptableObject定義
namespace Daifugo.Events    // EventChannel定義
namespace Daifugo.UI        // UI関連
namespace Daifugo.AI        // AI処理
```

### 実装例

```csharp
using UnityEngine;
using Tang3cko.EventChannels;

namespace Daifugo.Core
{
    public class GameManager : MonoBehaviour
    {
        // ...
    }
}

namespace Daifugo.UI
{
    public class GameScreenUI : MonoBehaviour
    {
        // ...
    }
}
```

---

## CreateAssetMenu命名規則

Daifugoプロジェクトでは、以下の命名規則を使用します：

```csharp
// データ系
[CreateAssetMenu(fileName = "Card", menuName = "Daifugo/Data/Card")]
[CreateAssetMenu(fileName = "Deck", menuName = "Daifugo/Data/Deck")]

// RuntimeSet系
[CreateAssetMenu(fileName = "PlayerRuntimeSet", menuName = "Daifugo/RuntimeSet/Player")]

// EventChannel系
[CreateAssetMenu(fileName = "CardEventChannel", menuName = "Daifugo/Events/Card Event Channel")]
[CreateAssetMenu(fileName = "VoidEventChannel", menuName = "Daifugo/Events/Void Event Channel")]
```

**パターン:**
- データ系: `"Daifugo/Data/カテゴリ"`
- RuntimeSet系: `"Daifugo/RuntimeSet/型名"`
- EventChannel系: `"Daifugo/Events/型名 Event Channel"`

---

## カードゲーム特有の実装

### CardSOの定義

```csharp
using UnityEngine;

namespace Daifugo.Data
{
    /// <summary>
    /// カードデータ（不変）
    /// </summary>
    [CreateAssetMenu(fileName = "Card", menuName = "Daifugo/Data/Card")]
    public class CardSO : ScriptableObject
    {
        [Header("Card Properties")]
        public Suit suit;           // スペード、ハート、ダイヤ、クラブ
        public int rank;            // 1-13（1=A, 11=J, 12=Q, 13=K）

        [Header("Visual")]
        public Sprite cardSprite;   // 2D版で使用
        public Material cardMaterial; // 3D版で使用（Phase 2）

        /// <summary>
        /// カードの強さを取得（大富豪ルール）
        /// </summary>
        public int GetStrength()
        {
            // 2 > A > K > ... > 3
            if (rank == 2) return 15;
            if (rank == 1) return 14;
            return rank;
        }

        public enum Suit
        {
            Spade,
            Heart,
            Diamond,
            Club
        }
    }
}
```

### DeckSOの定義

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace Daifugo.Data
{
    /// <summary>
    /// デッキ管理（ランタイム）
    /// </summary>
    [CreateAssetMenu(fileName = "Deck", menuName = "Daifugo/Data/Deck")]
    public class DeckSO : ScriptableObject
    {
        [SerializeField] private List<CardSO> allCards; // 52枚のカード
        private List<CardSO> deck = new List<CardSO>();

        public void Initialize()
        {
            deck.Clear();
            deck.AddRange(allCards);
            Shuffle();
        }

        public void Shuffle()
        {
            for (int i = 0; i < deck.Count; i++)
            {
                int randomIndex = Random.Range(i, deck.Count);
                (deck[i], deck[randomIndex]) = (deck[randomIndex], deck[i]);
            }
        }

        public CardSO DrawCard()
        {
            if (deck.Count == 0) return null;
            CardSO card = deck[0];
            deck.RemoveAt(0);
            return card;
        }

        public int RemainingCards => deck.Count;
    }
}
```

### CardDragManipulator実装

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using Tang3cko.EventChannels;

namespace Daifugo.UI
{
    /// <summary>
    /// カードのドラッグ&ドロップ操作（Phase 1 - UI Toolkit）
    /// </summary>
    public class CardDragManipulator : PointerManipulator
    {
        private Vector3 startPosition;
        private CardSO cardData;
        private CardEventChannelSO onCardPlayed;

        public CardDragManipulator(CardSO card, CardEventChannelSO eventChannel)
        {
            cardData = card;
            onCardPlayed = eventChannel;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            startPosition = target.transform.position;
            target.CapturePointer(evt.pointerId);
            target.AddToClassList("card--selected");
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (target.HasPointerCapture(evt.pointerId))
            {
                Vector3 delta = new Vector3(evt.deltaPosition.x, evt.deltaPosition.y, 0);
                target.transform.position += delta;
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (target.HasPointerCapture(evt.pointerId))
            {
                target.ReleasePointer(evt.pointerId);
                target.RemoveFromClassList("card--selected");

                // ドロップ位置判定
                if (IsOverPlayArea(evt.position))
                {
                    // EventChannel発火
                    onCardPlayed?.RaiseEvent(cardData);
                }
                else
                {
                    // 元の位置に戻す
                    target.transform.position = startPosition;
                }
            }
        }

        private bool IsOverPlayArea(Vector2 position)
        {
            VisualElement fieldArea = target.panel.visualTree.Q<VisualElement>("FieldCardsContainer");
            if (fieldArea == null) return false;
            return fieldArea.worldBound.Contains(position);
        }
    }
}
```

---

## Phase別の規約

### Phase 1（2D版）特有

- **UI Toolkit 100%使用**
- USSでスタイル定義
- PointerManipulatorでインタラクション
- Sprite画像でカード表示
- BEM命名規則の厳守

### Phase 2（3D版）特有

- **Phase 1のロジックを流用**
- UI Toolkit（HUD部分のみ）
- LitMotionでアニメーション
- Cinemachineでカメラワーク
- Material/Textureでカード表現

---

## ディレクトリ構造

### ScriptableObject配置

```
Assets/_Project/ScriptableObjects/
├── Cards/                  # 52枚のCardSO
│   ├── Spades/
│   ├── Hearts/
│   ├── Diamonds/
│   └── Clubs/
│
├── Data/
│   ├── Deck.asset
│   └── PlayerHand.asset
│
├── EventChannels/
│   ├── OnGameStarted.asset
│   ├── OnCardPlayed.asset
│   └── OnTurnChanged.asset
│
├── Variables/
│   ├── CurrentTurnIndex.asset
│   ├── SelectedCardCount.asset
│   └── IsRevolutionActive.asset
│
└── RuntimeSets/
    └── PlayerSet.asset
```

### スクリプト配置

```
Assets/_Project/Scripts/
├── Core/                  # ゲームロジック（Phase 1/2共通）
│   ├── GameManager.cs
│   ├── TurnManager.cs
│   ├── RuleValidator.cs
│   └── AIPlayer.cs
│
├── Data/                  # ScriptableObject定義
│   ├── CardSO.cs
│   ├── DeckSO.cs
│   └── PlayerHandSO.cs
│
├── Events/                # EventChannel定義
│   ├── CardEventChannelSO.cs
│   ├── IntEventChannelSO.cs
│   └── VoidEventChannelSO.cs
│
└── UI/                    # UI層
    ├── 2D/                # Phase 1
    │   ├── GameScreenUI.cs
    │   ├── CardUI.cs
    │   └── HandUI.cs
    └── 3D/                # Phase 2
        ├── Card3D.cs
        └── Hand3D.cs
```

---

## コードレビューチェックリスト

### アーキテクチャ

- [ ] ScriptableObjectの適切な使用
- [ ] EventChannelによる疎結合
- [ ] RuntimeSetによる動的管理
- [ ] 単一責任の原則の遵守
- [ ] Namespace規約の遵守（`Daifugo.*`）

### UI Toolkit（Phase 1）

- [ ] BEM命名規則の遵守
- [ ] USS変数の活用
- [ ] PointerManipulatorの適切な実装
- [ ] UXML/USSの分離

### パフォーマンス

- [ ] Update内の重い処理の回避
- [ ] キャッシングの実装
- [ ] イベント駆動設計

### 時間処理

- [ ] Time.deltaTimeの適切な使用
- [ ] フレームレート独立設計

### 品質

- [ ] null安全性の確保（`?.`演算子）
- [ ] エラーハンドリングの実装
- [ ] 適切なログ出力
- [ ] XMLドキュメントの記載

---

## 参考資料

### プロジェクト内ドキュメント

- **[共通コーディング規約](coding_standards/)** - 全プロジェクト共通のルール
- **[SpecKit](../00_spec/)** - 仕様駆動開発ワークフロー
- **[プロジェクト概要](../01_overview/)** - Daifugoプロジェクトの全体像

### 外部リソース

- **Unity UI Toolkit**: https://docs.unity3d.com/Manual/UIElements.html
- **Tang3cko.EventChannels**: EventChannelパッケージ
- **LitMotion**: https://github.com/AnnulusGames/LitMotion
- **Cinemachine**: https://docs.unity3d.com/Packages/com.unity.cinemachine@latest

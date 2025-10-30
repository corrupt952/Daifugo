# Unity Validation Attributes Package Idea

## Overview

SerializeField検証を自動化するためのUnityパッケージアイデア。Tang3cko.EventChannelsパッケージのような軽量でシンプルなフリーパッケージとして公開する可能性を検討。

**目的**: OnValidateを毎回書かずに、属性を付けるだけでSerializeFieldの検証を自動化する。

---

## Motivation

### 現状の問題点

```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private DeckSO deck;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private VoidEventChannelSO onGameStarted;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 毎回これを書くのが面倒...
        if (deck == null)
        {
            Debug.LogWarning($"[GameManager] deck is not assigned on {gameObject.name}.", this);
        }
        if (turnManager == null)
        {
            Debug.LogWarning($"[GameManager] turnManager is not assigned on {gameObject.name}.", this);
        }
        if (onGameStarted == null)
        {
            Debug.LogWarning($"[GameManager] onGameStarted is not assigned on {gameObject.name}.", this);
        }
    }
#endif
}
```

### 理想的な使い方

```csharp
public class GameManager : ValidatedMonoBehaviour
{
    [Required]
    [SerializeField] private DeckSO deck;

    [Required]
    [SerializeField] private TurnManager turnManager;

    [Required]
    [SerializeField] private VoidEventChannelSO onGameStarted;

    // OnValidateは基底クラスが自動実行！
}
```

---

## 既存パッケージ調査（2024年10月時点）

### 1. unity-inspector-required-field
- **GitHub**: https://github.com/jfranmora/unity-inspector-required-field
- **Stars**: 5 ⭐
- **License**: MIT
- **特徴**:
  - `[RequiredField]`属性でPlayモード開始時にnullチェック
  - UPM対応
- **欠点**:
  - 配列/リスト未対応
  - ネスト構造未対応
  - あまり活発ではない

### 2. unity-notnullattribute
- **GitHub**: https://github.com/redbluegames/unity-notnullattribute
- **Stars**: 113 ⭐
- **License**: MIT
- **特徴**:
  - `[NotNull]`属性
  - Inspector上で視覚的にエラー表示
  - `IgnorePrefab`フラグ対応
- **欠点**:
  - ⚠️ **メンテナンス停止** - 開発者がOdin Inspectorを推奨
  - .unitypackage形式（UPM非対応）

### 3. NaughtyAttributes
- **GitHub**: https://github.com/dbrizov/NaughtyAttributes
- **Stars**: 5,000 ⭐⭐⭐
- **License**: MIT
- **特徴**:
  - 多機能（Button, Dropdown, Slider等々）
  - OpenUPM対応
  - 広く使われている
- **欠点**:
  - 機能が多すぎる（シンプルさ重視なら不要）
  - 最終更新: 2022年2月（やや古い）
  - Required的な検証機能は一部のみ

### 4. scene-ref-attribute
- **GitHub**: https://github.com/KyleBanks/scene-ref-attribute
- **特徴**:
  - ValidatedMonoBehaviourパターン採用
  - InterfaceRef対応
  - OpenUPM対応
- **欠点**:
  - シーン内参照に特化（汎用的なRequiredには不向き）

---

## 提案: Tang3cko.ValidationAttributes パッケージ

### コンセプト

**"シンプルで軽量、Tang3cko.EventChannelsと相性が良い検証パッケージ"**

- EventChannelsと同じ哲学: 最小限の機能、最大限のシンプルさ
- リフレクションベースで拡張性が高い
- UPM + OpenUPM対応
- MIT License

---

## 実装案

### パッケージ構成

```
Tang3cko.ValidationAttributes/
├── Runtime/
│   ├── Attributes/
│   │   ├── RequiredAttribute.cs
│   │   ├── RequiredArrayAttribute.cs
│   │   └── ValidateMethodAttribute.cs (将来的に)
│   └── Core/
│       └── ValidatedMonoBehaviour.cs
├── Editor/
│   └── Drawers/
│       ├── RequiredFieldDrawer.cs (Inspector表示)
│       └── ValidationWindow.cs (全体検証ウィンドウ)
├── Tests/
│   └── Runtime/
│       └── ValidationTests.cs
├── package.json
├── README.md
└── LICENSE
```

### 実装コード例

#### RequiredAttribute.cs

```csharp
using System;

namespace Tang3cko.ValidationAttributes
{
    /// <summary>
    /// Marks a field as required (non-null) for validation
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiredAttribute : Attribute
    {
        /// <summary>
        /// Custom error message (optional)
        /// </summary>
        public string Message { get; set; }

        public RequiredAttribute() { }

        public RequiredAttribute(string message)
        {
            Message = message;
        }
    }
}
```

#### RequiredArrayAttribute.cs

```csharp
using System;

namespace Tang3cko.ValidationAttributes
{
    /// <summary>
    /// Marks an array/list field as required with length validation
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiredArrayAttribute : Attribute
    {
        public int MinLength { get; set; }
        public int MaxLength { get; set; } = int.MaxValue;

        public RequiredArrayAttribute(int minLength = 1)
        {
            MinLength = minLength;
        }
    }
}
```

#### ValidatedMonoBehaviour.cs

```csharp
using System.Reflection;
using UnityEngine;

namespace Tang3cko.ValidationAttributes
{
    /// <summary>
    /// Base MonoBehaviour with automatic field validation
    /// Validates all SerializeFields marked with validation attributes
    /// </summary>
    public abstract class ValidatedMonoBehaviour : MonoBehaviour
    {
#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            ValidateFields();
        }

        private void ValidateFields()
        {
            var type = GetType();
            var fields = type.GetFields(
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public
            );

            foreach (var field in fields)
            {
                ValidateRequiredField(field);
                ValidateRequiredArray(field);
            }
        }

        private void ValidateRequiredField(FieldInfo field)
        {
            var requiredAttr = field.GetCustomAttribute<RequiredAttribute>();
            if (requiredAttr == null) return;

            var value = field.GetValue(this);

            if (value == null || (value is Object unityObj && unityObj == null))
            {
                string message = requiredAttr.Message ??
                    $"Required field '{field.Name}' is not assigned";

                Debug.LogWarning(
                    $"[{GetType().Name}] {message} on {gameObject.name}.",
                    this
                );
            }
        }

        private void ValidateRequiredArray(FieldInfo field)
        {
            var arrayAttr = field.GetCustomAttribute<RequiredArrayAttribute>();
            if (arrayAttr == null) return;

            var value = field.GetValue(this);

            if (value == null)
            {
                Debug.LogWarning(
                    $"[{GetType().Name}] Required array '{field.Name}' is null on {gameObject.name}.",
                    this
                );
                return;
            }

            if (value is System.Collections.ICollection collection)
            {
                int length = collection.Count;

                if (length < arrayAttr.MinLength)
                {
                    Debug.LogWarning(
                        $"[{GetType().Name}] Array '{field.Name}' has {length} elements but requires at least {arrayAttr.MinLength} on {gameObject.name}.",
                        this
                    );
                }

                if (length > arrayAttr.MaxLength)
                {
                    Debug.LogWarning(
                        $"[{GetType().Name}] Array '{field.Name}' has {length} elements but maximum is {arrayAttr.MaxLength} on {gameObject.name}.",
                        this
                    );
                }
            }
        }
#endif
    }
}
```

### 使用例

```csharp
using Tang3cko.ValidationAttributes;
using UnityEngine;

public class GameManager : ValidatedMonoBehaviour
{
    [Required]
    [SerializeField] private DeckSO deck;

    [Required("TurnManager is essential for game flow")]
    [SerializeField] private TurnManager turnManager;

    [RequiredArray(minLength: 4, MaxLength = 4)]
    [SerializeField] private PlayerHandSO[] playerHands;

    // 通常のSerializeField（検証なし）
    [SerializeField] private float gameSpeed = 1.0f;

    // OnValidateは基底クラスが自動実行
    // 必要に応じてoverrideして追加検証も可能
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate(); // 基底クラスの検証を実行

        // カスタム検証ロジック
        if (gameSpeed <= 0)
        {
            Debug.LogWarning("[GameManager] gameSpeed must be positive.");
        }
    }
#endif
}
```

---

## 他パッケージとの差別化

### vs NaughtyAttributes
- ✅ より軽量（検証機能のみ）
- ✅ 最新Unityバージョンサポート
- ✅ EventChannelsとの統一感
- ✅ アクティブメンテナンス

### vs unity-notnullattribute
- ✅ メンテナンス継続
- ✅ UPM対応
- ✅ 配列検証対応
- ✅ カスタムメッセージ対応

### vs unity-inspector-required-field
- ✅ 配列/リスト対応
- ✅ より詳細なエラーメッセージ
- ✅ ValidatedMonoBehaviourパターン
- ✅ 拡張性が高い

---

## 技術仕様

### 依存関係
- Unity 2021.3 以降（LTS）
- .NET Standard 2.1

### パフォーマンス
- リフレクション使用（Editor専用なのでランタイム影響なし）
- OnValidate実行時のみ（編集時のみ）

### 互換性
- ScriptableObject対応（ValidatedScriptableObjectも提供）
- カスタムEditor対応
- Prefab/Scene両対応

---

## 公開計画

### Phase 1: MVP
- [ ] RequiredAttribute実装
- [ ] ValidatedMonoBehaviour実装
- [ ] 基本テスト作成
- [ ] README作成

### Phase 2: 拡張機能
- [ ] RequiredArrayAttribute実装
- [ ] Inspector Drawer実装
- [ ] ValidatedScriptableObject実装
- [ ] サンプルシーン追加

### Phase 3: 公開
- [ ] GitHub公開
- [ ] OpenUPM登録
- [ ] ドキュメント整備
- [ ] ブログ記事執筆

### Phase 4: 継続改善
- [ ] コミュニティフィードバック対応
- [ ] 新規検証属性追加
- [ ] Unity新バージョン対応

---

## メリット・デメリット

### ✅ メリット

#### 開発者として
- OnValidate地獄から解放される
- 属性を付けるだけ（1行追加）
- コードレビュー時の見落とし防止
- チーム開発での統一感

#### パッケージ公開者として
- EventChannelsとのシナジー（同じ開発者のブランド）
- ニッチだが需要はある
- OSSコントリビューション
- ポートフォリオとして

### ⚠️ デメリット

#### 技術的
- リフレクション使用（Editor専用なので実質問題なし）
- 基底クラスへの依存
- 複雑な検証には不向き

#### 開発・保守
- メンテナンスコスト
- Unity新バージョン対応の必要性
- ユーザーサポート対応

---

## 競合分析まとめ

| パッケージ | Stars | メンテ | UPM | 軽量性 | 配列対応 | 評価 |
|-----------|-------|--------|-----|--------|----------|------|
| NaughtyAttributes | 5,000⭐⭐⭐ | △ | ✅ | △ | △ | 多機能すぎ |
| unity-notnullattribute | 113⭐ | ❌ | ❌ | ✅ | ❌ | 停止 |
| unity-inspector-required | 5⭐ | △ | ✅ | ✅ | ❌ | 機能不足 |
| **Tang3cko.Validation** | - | ✅ | ✅ | ✅ | ✅ | **理想** |

---

## 結論

### 公開する価値はある？

**YES** - 以下の理由から：

1. **既存パッケージの隙間を埋める**
   - NaughtyAttributesは多機能すぎる
   - unity-notnullattributeは停止
   - シンプルで軽量な選択肢が少ない

2. **Tang3cko.EventChannelsとの相性**
   - 同じ哲学（シンプル、軽量、実用的）
   - セットで使うと開発体験が向上
   - ブランド統一感

3. **実用性が高い**
   - OnValidateは全Unityプロジェクトで必要
   - 実装は簡単だが毎回書くのは面倒
   - チーム開発で特に有用

### 推奨アクション

1. **まずDaifugoで実装して検証**
   - 実際のプロジェクトで使ってみる
   - 問題点を洗い出す
   - API設計をブラッシュアップ

2. **MVP版を作成**
   - Required + ValidatedMonoBehaviourのみ
   - シンプルに始める

3. **GitHub公開**
   - 反応を見る
   - フィードバック収集

4. **必要に応じて拡張**
   - コミュニティからの要望対応
   - 新機能追加

---

## 参考リンク

- Tang3cko.EventChannels: https://github.com/tang3cko/EventChannels (仮)
- NaughtyAttributes: https://github.com/dbrizov/NaughtyAttributes
- unity-notnullattribute: https://github.com/redbluegames/unity-notnullattribute
- Unity Attributes Documentation: https://docs.unity3d.com/Manual/Attributes.html

---

## 更新履歴

- 2024-10-30: 初版作成、既存パッケージ調査完了

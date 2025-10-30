# Daifugo - Project Context for Claude Code

## Purpose

このドキュメントは、Daifugoプロジェクト（大富豪カードゲーム）におけるClaude Code向けのプロジェクトコンテキストを提供します。SpecKit (Spec-Driven Development) ワークフロー、ドキュメント構造、重要なルールを明確化します。

## Checklist

- [ ] コーディング開始前に `docs/03_technical/coding_standards.md` を読み込む
- [ ] 対象機能の仕様書（spec/plan/tasks）を確認する
- [ ] SpecKitワークフロー（specify → clarify → plan → tasks → implement）に従う
- [ ] 全フェーズでコーディング規約を厳守する
- [ ] 実装完了後にドキュメントを更新する

---

## プロジェクト概要

**Daifugo（大富豪）** は、Unity練習として作成するトランプカードゲームの学習プロジェクトです。

### 開発アプローチ

**2D（UI Toolkit）→ 3D（演出追加）** の段階的アプローチで、異なる技術スタックを学習します。

- **Phase 1**: 2D版（UI Toolkit完全ベース）- 1-2週間
- **Phase 2**: 3D版（演出強化）- 1週間

### 学習目標

1. UI Toolkitの深い理解（ドラッグ&ドロップ、動的リスト表示、USSアニメーション）
2. 2D開発経験の獲得（カードゲームで2D開発の基礎を固める）
3. 既存アーキテクチャの実践（ScriptableObject、EventChannel、RuntimeSetパターン）
4. 3D演出の学習（3D空間でのカード配置、カメラワーク、アニメーション）

### 技術スタック概要

- **Phase 1**: UI Toolkit 100%、ScriptableObject、EventChannel
- **Phase 2**: 3D URP、LitMotion、Cinemachine

---

## SpecKit Workflow

このプロジェクトは **SpecKit (Spec-Driven Development)** に基づいて開発されています。

**SpecKit本家**: https://github.com/github/spec-kit

### ワークフロー

全機能開発で以下の5ステップを実行します：

1. **`/specify`** - 機能要件を明確に定義（`docs/00_spec/`）
2. **`/clarify`** - 曖昧性を解消（`docs/00_spec/clarifications.md`）
3. **`/plan`** - 技術実装計画を策定（`docs/98_plans/`）
4. **`/tasks`** - 実装タスクに分解（`docs/98_plans/tasks/`）
5. **`/implement`** - タスクを実行して実装

### 新機能開発時の手順

1. `docs/00_spec/[feature]-spec.md` を作成（/specify）
2. `docs/00_spec/clarifications.md` を更新（/clarify）
3. `docs/98_plans/[feature]-plan.md` を作成（/plan）
4. `docs/98_plans/tasks/[feature]-tasks.md` を作成（/tasks）
5. タスクを順次実装（/implement）
6. 実装完了後、ドキュメントを更新

---

## Documentation Structure

```
docs/
├── 00_spec/              # SpecKit仕様書（specify/clarify）
│   ├── clarifications/   # 設計判断のサブディレクトリ
│   └── clarifications.md # メイン設計判断ドキュメント
├── 01_overview/          # プロジェクト概要
├── 02_game_design/       # ゲームデザイン仕様
├── 03_technical/         # 技術仕様・コーディング規約
│   ├── coding_standards.md        # Daifugo固有のコーディング規約
│   └── coding_standards/          # 共通コーディング規約
│       ├── core/                  # 基本規約
│       ├── architecture/          # アーキテクチャパターン
│       ├── ui/                    # UI実装規約
│       ├── documentation/         # ドキュメント書き方ガイド
│       └── examples/              # コード例
├── 98_plans/             # 実装計画（plan）
│   └── tasks/            # タスク分解（tasks）
└── 99_ideas/             # アイデアメモ
```

---

## Critical Rules

### 🚨 タスク開始前の必須確認事項

**どんなタスクでも、作業を開始する前に必ず関連ドキュメントを読み込むこと。**

このプロジェクトのドキュメントには、以下のあらゆる側面について詳細なルールとガイドラインが記載されています：

- **コーディング規約** - 命名規則、アーキテクチャパターン、パフォーマンス最適化
- **プロジェクトの進め方** - SpecKitワークフロー、機能開発フロー
- **ドキュメントの書き方** - 構造、フォーマット、コード例の書き方
- **テストの書き方** - テスト原則、パターン、よくある落とし穴
- **UI実装規約** - UI Toolkit、BEM命名、レスポンシブ対応
- **エラーハンドリング** - null安全性、例外処理、ログ出力

**参考情報ではありません。これらは必ず従うべきグランドルールです。**

### ドキュメント読み込みルール

コーディング開始前に必ず以下を読み込むこと：

1. `docs/03_technical/coding_standards.md` - Daifugo固有のコーディング規約（絶対準拠）
2. `docs/03_technical/coding_standards/README.md` - 共通コーディング規約の全体像（必読）
3. `docs/00_spec/clarifications.md` - 設計判断の理由
4. 対象機能の仕様書:
   - `docs/00_spec/[feature]-spec.md`
   - `docs/98_plans/[feature]-plan.md`
   - `docs/98_plans/tasks/[feature]-tasks.md`

**タスクの種類に応じて追加で読むべきドキュメント：**

- **新規実装タスク**: architecture/ 配下の全ファイル
- **UI実装タスク**: ui/ 配下の該当ファイル
- **テスト実装タスク**: testing/ 配下の全ファイル
- **ドキュメント作成タスク**: documentation/ 配下の全ファイル

### コーディング規約の絶対準拠

- 全フェーズで厳守: プロトタイプ・本番の区別なく規約に従う
- 違反禁止: 「プロトタイプだから規約違反OK」は絶対NG
- 参照: `docs/03_technical/coding_standards.md`

### SpecKitワークフローの厳守

- 全機能開発で5ステップ（specify → clarify → plan → tasks → implement）を実行
- ドキュメントを先に作成し、それに基づいて実装
- 実装完了後、必ずドキュメントを更新

---

## References

- **[Coding Standards](docs/03_technical/coding_standards.md)** - Daifugo固有のコーディング規約
- **[共通コーディング規約](docs/03_technical/coding_standards/README.md)** - 全プロジェクト共通ルール（ディレクトリ構造、ファイル一覧）
- **[SpecKit GitHub](https://github.com/github/spec-kit)** - Spec-Driven Development本家
- **[SpecKit Quick Start](https://github.com/github/spec-kit#quick-start)** - ワークフロー詳細
- **[SpecKit Templates](https://github.com/github/spec-kit/tree/main/templates)** - spec/plan/tasksテンプレート
- **[Project Ideas](docs/99_ideas/daifugo_card_game_project.md)** - Rookieプロジェクトから継承したアイデアメモ

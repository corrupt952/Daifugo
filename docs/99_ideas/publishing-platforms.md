# ゲーム公開プラットフォーム検討メモ

## 背景

UI Toolkit練習プロジェクトとして作成したDaifugoを、どこかのプラットフォームに公開する検討。

## 検討したプラットフォーム

### itch.io
- **特徴**: 国際的なインディーゲームプラットフォーム
- **対応ビルド**: WebGL、Windows、Mac、Linux、Android等（複数ビルド同時公開可能）
- **プレイ方法**: ブラウザプレイ or ダウンロード
- **デザイン**: 高度なカスタマイズ可能（標準テーマエディタ + CSS申請）
- **個人ページ**: `ユーザー名.itch.io` という独自URL
- **コミュニティ**: 英語圏が中心、国際的なリーチ

### unityroom
- **特徴**: 日本のUnity専用ゲーム投稿プラットフォーム
- **対応ビルド**: WebGLのみ
- **プレイ方法**: ブラウザプレイのみ
- **デザイン**: 統一されたプラットフォームデザイン（カスタマイズ不可）
- **コミュニティ**: 日本語、Unity開発者コミュニティ
- **イベント**: Unity1週間ゲームジャムなど定期開催

## 結論

**itch.io をメインプラットフォームとして採用**

### 理由
1. **複数ビルド対応**: WebGL + Windows版など柔軟に公開可能
2. **ポートフォリオ**: 個人ページとして機能
3. **段階的開発**: Phase 1（2D）→ Phase 2（3D）の進化を同じページで管理可能
4. **両プラットフォーム併用も可能**: unityroomにもWebGL版を公開して日本語フィードバック収集

## アセット利用規約

### Kenneyアセット

- **ライセンス**: CC0（パブリックドメイン）
- **商用利用**: OK
- **クレジット表記**: 不要（推奨）
- **改変**: OK
- **プラットフォーム制限**: なし

#### クレジット記載例

```markdown
## Credits
- Card Assets: Kenney's Playing Cards Pack (CC0 License)
- https://kenney.nl/
```

### Synty Studiosアセット

- **ライセンス**: 購入時のEULA（一回購入またはサブスクリプション）
- **商用利用**: OK（ゲームへの組み込みのみ）
- **クレジット表記**: 不要（任意）
- **プラットフォーム制限**: なし（itch.io、Steam、unityroom等すべてOK）

#### できること ✅
- ゲームへの組み込みと公開・配布・販売
- アセットの改変（Maya、Blender等で編集可能）
- 各プラットフォーム（PC/モバイル/コンソール/WebGL）への公開

#### 禁止事項 ❌
- アセット単体での再配布・再販売
- ソースファイルをチーム外に共有
- Synty公式の宣伝素材（スクリーンショット/トレーラー）の流用
- 自分が作成したと虚偽の主張
- NFT、ブロックチェーン、メタバース系プラットフォーム、生成AI用途

#### 重要な注意
- Unity Learnの無料Syntyアセットは商用利用不可
- 商用利用する場合はUnity Asset StoreまたはSynty公式ストアから購入が必要

#### クレジット記載例（任意）

```markdown
## Credits
- 3D Assets: Synty Studios
- https://www.syntystudios.com/
```

## itch.io公開時のチェックリスト

- [ ] ゲームタイトル
- [ ] サムネイル画像（630x500px推奨）
- [ ] スクリーンショット（3-5枚）
- [ ] 説明文（英語 or 英語+日本語）
- [ ] Kenneyクレジット表記
- [ ] タグ設定（card-game, daifugo等）
- [ ] ビルドアップロード（WebGL + Windows等）

## 参考リンク

### プラットフォーム
- itch.io公式ドキュメント: https://itch.io/docs/creators/getting-started
- unityroom利用規約: https://unityroom.com/terms

### アセット
- Kenney公式サイト: https://kenney.nl/
- Synty Studios公式サイト: https://www.syntystudios.com/
- Synty Store EULA: https://syntystore.com/pages/end-user-licence-agreement
- Syntyアセット使用ゲーム例（itch.io）: https://itch.io/c/1683510/games-using-syntys-assets

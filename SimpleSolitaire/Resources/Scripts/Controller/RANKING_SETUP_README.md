# UGS 週間ランキング機能 - Unity エディタ設定手順

## 概要
この手順書では、スクリプト作成後に Unity エディタで行う設定を説明します。

---

## 1. LeaderboardManager オブジェクトの作成

1. **Hierarchy** で右クリック → **Create Empty**
2. 名前を `LeaderboardManager` に変更
3. **Inspector** で **Add Component** → `LeaderboardManager` スクリプトをアタッチ

> このオブジェクトは `DontDestroyOnLoad` で自動的に永続化されます。

---

## 2. RankingLayer Prefab の作成

### 2-1. 既存の StatisticsLayer を複製

1. `Assets/SimpleSolitaire/Resources/Prefabs/Layers/StatisticsLayer.prefab` を選択
2. **Ctrl+D (Cmd+D)** で複製
3. 名前を `RankingLayer` に変更

### 2-2. RankingLayer の構成を編集

1. `RankingLayer` をダブルクリックして Prefab 編集モードに入る
2. 以下の構成に変更:

```
RankingLayer (GameObject + Animator)
├── Background (Image - 背景)
├── Panel (RectTransform)
│   ├── Title (Text) - "WEEKLY RANKING"
│   ├── Content (RectTransform + Vertical Layout Group)
│   │   └── (ランキングエントリが動的に追加される)
│   ├── PlayerRank (Text) - "YOUR RANK: --"
│   ├── LoadingText (Text) - "Loading..."
│   ├── ErrorText (Text) - エラー表示用
│   └── CloseButton (Button) - 閉じるボタン
```

### 2-3. RankingEntryPrefab の作成

1. `Assets/SimpleSolitaire/Resources/Prefabs/` に新規 Prefab を作成
2. 名前を `RankingEntry` に変更
3. 構成:

```
RankingEntry (GameObject + Horizontal Layout Group)
├── RankText (Text) - 順位表示 (例: "1")
├── NameText (Text) - プレイヤー名
└── TimeText (Text) - クリアタイム (例: "02:35")
```

### 2-4. RankingUI コンポーネントのアタッチ

1. `RankingLayer` に `RankingUI` スクリプトをアタッチ
2. Inspector で以下を設定:
   - **Ranking Content**: Content オブジェクトをドラッグ
   - **Ranking Entry Prefab**: RankingEntry Prefab をドラッグ
   - **Player Rank Text**: PlayerRank Text をドラッグ
   - **Loading Text**: LoadingText をドラッグ
   - **Error Text**: ErrorText をドラッグ
   - **Ranking Count**: 10 (表示件数)

---

## 3. シーンへの配置

### 3-1. RankingLayer の配置

1. シーン `GameScene_Klondike(UndoCountable)` を開く
2. Canvas 内に `RankingLayer` Prefab を配置
3. 初期状態で **非アクティブ** に設定

### 3-2. GameManager への参照設定

1. GameManager オブジェクトを選択
2. Inspector の **Layers** セクションで:
   - **Ranking Layer**: 配置した RankingLayer をドラッグ
3. **Ranking** セクションで:
   - **Ranking UI**: RankingLayer の RankingUI コンポーネントをドラッグ

---

## 4. ボタンの設定

### 4-1. 設定画面に「Ranking」ボタンを追加

1. `SettingsLayer` Prefab を編集
2. 既存の Statistics ボタンを複製
3. テキストを "RANKING" に変更
4. **OnClick** イベントに `GameManager.OnClickSettingLayerRankingBtn` を設定

### 4-2. Win画面に「Ranking」ボタンを追加

1. `WinLayer` Prefab を編集
2. 新規ボタンを追加（または既存ボタンを複製）
3. テキストを "RANKING" に変更
4. **OnClick** イベントに `GameManager.OnClickWinLayerRankingBtn` を設定

### 4-3. RankingLayer の閉じるボタン設定

1. `RankingLayer` の CloseButton を選択
2. **OnClick** イベントに以下のいずれかを設定:
   - 設定画面に戻る場合: `GameManager.OnClickRankingLayerCloseBtn`
   - ゲーム画面に戻る場合: `GameManager.OnClickRankingLayerBackToGameBtn`

---

## 5. Animator 設定

RankingLayer には既存の Layer と同様の Animator が必要です。

1. `Assets/SimpleSolitaire/Resources/Animation/Layer/LayerAnimator.controller` を RankingLayer の Animator にアサイン
2. または StatisticsLayer から複製した場合は既に設定されています

---

## 6. テスト

1. Unity エディタでプレイ
2. ゲームをクリアしてスコアが送信されることを確認
3. 設定画面から「Ranking」ボタンでランキングが表示されることを確認
4. Win画面から「Ranking」ボタンでランキングが表示されることを確認

---

## トラブルシューティング

### "Not initialized yet" のエラーが出る
- LeaderboardManager が正しく配置されているか確認
- Unity Dashboard でプロジェクトがリンクされているか確認
- インターネット接続を確認

### ランキングが表示されない
- Unity Dashboard の Leaderboards で `weekly_clear_time` が作成されているか確認
- Sort Order が **Ascending** になっているか確認

### プレイヤー名が "Player_xxxxxxxx" と表示される
- 匿名認証のため、Player ID の一部が表示されます
- プレイヤー名を設定する場合は `AuthenticationService.Instance.UpdatePlayerNameAsync("Name")` を使用

---

## ファイル一覧

| ファイル | 説明 |
|----------|------|
| `LeaderboardManager.cs` | UGS 初期化・スコア送信・ランキング取得 |
| `RankingUI.cs` | ランキング表示 UI ロジック |
| `GameManager.cs` (修正) | クリア時スコア送信、ランキング画面制御 |
| `RankingLayer.prefab` (新規作成) | ランキング表示 UI |
| `RankingEntry.prefab` (新規作成) | ランキング1行のテンプレート |


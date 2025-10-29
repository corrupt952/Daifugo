# Phase 1: 技術実装計画

## 目的

phase1-spec.mdとclarifications.mdに基づき、技術実装の詳細を計画する。

---

## アーキテクチャ概要

### レイヤー構成

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
└──────────────┬──────────────────────┘
               │ Access
┌──────────────┴──────────────────────┐
│         Data Layer                   │
│  CardSO, DeckSO, PlayerHandSO        │
└─────────────────────────────────────┘
```

### 設計原則
1. **完全なイベント駆動**: Update()ループなし、全てEventChannelで通信
2. **疎結合**: レイヤー間はEventChannelのみで通信
3. **ScriptableObject中心**: データと設定は全てSO
4. **単一責任**: 各コンポーネントは1つの責務のみ

### 初期化順序とイベントタイミング

**Unityのライフサイクル順序**:
```
1. Awake()        - UI要素の取得、初期状態設定
2. OnEnable()     - イベント購読
3. Start()        - ゲーム開始処理
4. イベント発火    - OnGameStarted → OnTurnChanged
```

**イベント順序保証**:
- OnEnable()は Start()より先に実行されるため、全コンポーネントは Start()でイベント発火する前に購読完了
- GameManager.Start() → StartGame() → onGameStarted.RaiseEvent() → turnManager.Initialize() → onTurnChanged.RaiseEvent()
- この順序により、全てのサブスクライバーはイベントを受け取れる

**NullReferenceException対策**:
- 全イベントハンドラで null チェック実施（例: `playerHandUI?.GetSelectedCards()`）
- OnGameStarted で各コンポーネントの初期化完了を保証
- ゲーム開始前にイベントが発火しないよう、GameManager.Start()で制御

---

## データ層

### CardSO (ScriptableObject)

**責務**: 1枚のカードデータを保持

**プロパティ**:
```csharp
public enum Suit { Spade, Heart, Diamond, Club }

[SerializeField] private Suit cardSuit;
[SerializeField] private int rank; // 1-13 (1=A, 11=J, 12=Q, 13=K)
[SerializeField] private Sprite cardSprite;
```

**メソッド**:
```csharp
public int GetStrength()
{
    if (rank == 2) return 15; // 2が最強
    if (rank == 1) return 14; // Aが2番目
    return rank; // 3-13はそのまま
}
```

**アセット配置**:
```
Assets/_Project/ScriptableObjects/Cards/
├── Spades/
│   ├── Spade_A.asset
│   ├── Spade_2.asset
│   └── ... (13枚)
├── Hearts/ (13枚)
├── Diamonds/ (13枚)
└── Clubs/ (13枚)
```

---

### DeckSO (ScriptableObject)

**責務**: 52枚のカードを管理、シャッフル、配布

**プロパティ**:
```csharp
[SerializeField] private List<CardSO> allCards; // Inspector設定（52枚）
private List<CardSO> currentDeck; // ランタイム用シャッフル済みデック

public IReadOnlyList<CardSO> CurrentDeck => currentDeck; // バリデーション用
```

**メソッド**:
```csharp
public void Initialize()
{
    // バリデーション
    if (allCards == null || allCards.Count != 52)
    {
        Debug.LogError($"DeckSO.Initialize: allCards must contain exactly 52 cards. Current count: {allCards?.Count ?? 0}");
        return;
    }

    currentDeck = new List<CardSO>(allCards);
    Shuffle();
}

public void Shuffle()
{
    // Fisher-Yates shuffle
    for (int i = currentDeck.Count - 1; i > 0; i--)
    {
        int j = Random.Range(0, i + 1);
        (currentDeck[i], currentDeck[j]) = (currentDeck[j], currentDeck[i]);
    }
}

public CardSO DrawCard()
{
    if (currentDeck.Count == 0) return null;
    CardSO card = currentDeck[0];
    currentDeck.RemoveAt(0);
    return card;
}

public void DistributeCards(PlayerHandSO[] hands)
{
    // ラウンドロビンで配布
    int handIndex = 0;
    while (currentDeck.Count > 0)
    {
        CardSO card = DrawCard();
        hands[handIndex].AddCard(card);
        handIndex = (handIndex + 1) % hands.Length;
    }
}
```

---

### PlayerHandSO (ScriptableObject)

**責務**: プレイヤーの手札を管理

**プロパティ**:
```csharp
[SerializeField] private int playerID; // 0-3
private List<CardSO> cardsInHand = new();
```

**メソッド**:
```csharp
public void Initialize()
{
    cardsInHand.Clear();
}

public void AddCard(CardSO card)
{
    cardsInHand.Add(card);
}

public void RemoveCard(CardSO card)
{
    cardsInHand.Remove(card);
}

public void SortByStrength()
{
    cardsInHand = cardsInHand
        .OrderBy(c => c.GetStrength())
        .ToList();
}

public List<CardSO> GetPlayableCards(int fieldStrength)
{
    if (fieldStrength == 0)
        return new List<CardSO>(cardsInHand);

    return cardsInHand
        .Where(c => c.GetStrength() > fieldStrength)
        .ToList();
}

public IReadOnlyList<CardSO> Cards => cardsInHand;
public int CardCount => cardsInHand.Count;
public bool IsEmpty => cardsInHand.Count == 0;
```

---

## イベント層

### EventChannels

**VoidEventChannelSO**:
- OnGameStarted
- OnPassButtonClicked
- OnFieldReset

**IntEventChannelSO**:
- OnTurnChanged(int playerID)
- OnGameEnded(int winnerID)

**CardEventChannelSO** (既存のGenericEventChannelSO<CardSO>):
- OnPlayCardRequested(CardSO card) - UI/AIがカードをプレイしたい時に発火（コマンドイベント）

**カスタムEventChannel**:

```csharp
// CardPlayedEventChannelSO.cs
public struct CardPlayedEventData
{
    public CardSO Card;        // プレイされたカード
    public int PlayerID;       // プレイしたプレイヤー
    public CardSO FieldCard;   // プレイ後の場のカード（通常はCardと同じ）
}

[CreateAssetMenu(menuName = "Events/CardPlayedEventChannel")]
public class CardPlayedEventChannelSO : GenericEventChannelSO<CardPlayedEventData> { }
```

- OnCardPlayed(CardPlayedEventData) - カードが正常にプレイされた後に GameManager が発火（通知イベント）
  - AIController はこれを購読して currentFieldCard をローカルで追跡
  - GameScreenUI はこれを購読して画面を更新

### Variables

**Phase 1ではVariablesを使用しない**（C-008の設計判断に従う）

理由:
- 全ての状態はイベント経由で通知
- 各コンポーネントはローカル変数で状態を保持
- 完全な疎結合を実現

---

## ゲームロジック層

### GameManager (MonoBehaviour)

**責務**: ゲーム全体の制御

**SerializeField**:
```csharp
[Header("Data")]
[SerializeField] private DeckSO deck;
[SerializeField] private PlayerHandSO[] playerHands; // 4要素

[Header("Events - Commands")]
[SerializeField] private CardEventChannelSO onPlayCardRequested;
[SerializeField] private VoidEventChannelSO onPassButtonClicked;

[Header("Events - Notifications")]
[SerializeField] private VoidEventChannelSO onGameStarted;
[SerializeField] private CardPlayedEventChannelSO onCardPlayed;
[SerializeField] private VoidEventChannelSO onFieldReset;
[SerializeField] private IntEventChannelSO onGameEnded;

[Header("Components")]
[SerializeField] private TurnManager turnManager;
[SerializeField] private RuleValidator ruleValidator;
```

**ランタイム状態**:
```csharp
private CardSO currentFieldCard;
private bool isGameActive;
```

**イベント購読**:
```csharp
private void OnEnable()
{
    // コマンドイベントを購読
    onPlayCardRequested.OnEventRaised += HandlePlayCardRequested;
    onPassButtonClicked.OnEventRaised += HandlePassButtonClicked;
    onFieldReset.OnEventRaised += HandleFieldReset;
}

private void OnDisable()
{
    onPlayCardRequested.OnEventRaised -= HandlePlayCardRequested;
    onPassButtonClicked.OnEventRaised -= HandlePassButtonClicked;
    onFieldReset.OnEventRaised -= HandleFieldReset;
}
```

**主要メソッド**:
```csharp
private void Start()
{
    StartGame();
}

public void StartGame()
{
    // 1. 初期化
    currentFieldCard = null;
    isGameActive = true;

    // 2. 手札クリア
    foreach (var hand in playerHands)
        hand.Initialize();

    // 3. デッキ初期化・配布
    deck.Initialize();
    if (deck.CurrentDeck.Count == 0) return; // バリデーション失敗時は中断

    deck.DistributeCards(playerHands);

    // 4. 手札ソート
    foreach (var hand in playerHands)
        hand.SortByStrength();

    // 5. イベント発火（UI初期化）
    onGameStarted.RaiseEvent();

    // 6. ターン開始（TurnManagerが初期化され、OnTurnChangedイベントが発火される）
    turnManager.Initialize();
}

private void HandlePlayCardRequested(CardSO card)
{
    if (!isGameActive) return;

    int currentPlayer = turnManager.GetCurrentPlayer();
    PlayerHandSO hand = playerHands[currentPlayer];

    // 1. バリデーション
    if (!ruleValidator.IsCardInHand(card, hand))
    {
        Debug.LogWarning($"Card {card.name} is not in player {currentPlayer}'s hand");
        return;
    }

    if (!ruleValidator.CanPlayCard(card, currentFieldCard))
    {
        Debug.LogWarning($"Cannot play {card.name} on {currentFieldCard?.name ?? "empty field"}");
        return;
    }

    // 2. カードプレイ処理
    hand.RemoveCard(card);
    currentFieldCard = card;

    // 3. 通知イベント発火（UIと他のコンポーネントに通知）
    var playedData = new CardPlayedEventData
    {
        Card = card,
        PlayerID = currentPlayer,
        FieldCard = currentFieldCard
    };
    onCardPlayed.RaiseEvent(playedData);

    // 4. 勝利判定
    if (hand.IsEmpty)
    {
        EndGame(currentPlayer);
        return;
    }

    // 5. ターン進行（TurnManagerに通知）
    turnManager.OnCardPlayed(currentPlayer);
}

private void HandlePassButtonClicked()
{
    if (!isGameActive) return;

    // 現在のプレイヤーが人間プレイヤー（0）かチェック
    int currentPlayer = turnManager.GetCurrentPlayer();
    if (currentPlayer != 0)
    {
        Debug.LogWarning($"Pass button clicked but current player is {currentPlayer}, not human player 0");
        return;
    }

    // パス処理（TurnManagerに通知）
    turnManager.OnPlayerPass();
}

private void HandleFieldReset()
{
    // 場をクリア
    currentFieldCard = null;
}

private void EndGame(int winnerID)
{
    isGameActive = false;
    onGameEnded.RaiseEvent(winnerID);
}
```

**Public API** (TurnManagerが使用):
```csharp
// DeckSOのcurrentDeckが空でないかをチェックするためのプロパティが必要
// DeckSO側に public IReadOnlyList<CardSO> CurrentDeck => currentDeck; を追加
```

---

### TurnManager (MonoBehaviour)

**責務**: ターン進行と場の流れ管理

**SerializeField**:
```csharp
[SerializeField] private IntEventChannelSO onTurnChanged;
[SerializeField] private VoidEventChannelSO onFieldReset;
```

**ランタイム状態**:
```csharp
private int currentPlayerID = 0;
private int lastCardPlayerID = -1; // 最後にカードを出したプレイヤー（親）
private HashSet<int> passedPlayers = new(); // パスしたプレイヤー
private const int PLAYER_COUNT = 4;
```

**主要メソッド**:
```csharp
public void Initialize()
{
    currentPlayerID = 0; // プレイヤー0から開始
    lastCardPlayerID = -1; // ゲーム開始時は親なし
    passedPlayers.Clear();
    onTurnChanged.RaiseEvent(0);
}

public void NextTurn()
{
    currentPlayerID = (currentPlayerID + 1) % PLAYER_COUNT;

    // 場リセット判定（親に戻った時）
    if (lastCardPlayerID != -1 && currentPlayerID == lastCardPlayerID)
    {
        // 親以外の全員がパスしたか
        if (passedPlayers.Count == PLAYER_COUNT - 1)
        {
            ResetField();
        }
    }

    onTurnChanged.RaiseEvent(currentPlayerID);
}

public void OnCardPlayed(int playerID)
{
    lastCardPlayerID = playerID; // 親を更新
    passedPlayers.Clear(); // パス記録クリア
    NextTurn();
}

public void OnPlayerPass()
{
    passedPlayers.Add(currentPlayerID); // パス記録
    NextTurn();
}

private void ResetField()
{
    lastCardPlayerID = -1; // 親をクリア
    passedPlayers.Clear();
    onFieldReset.RaiseEvent();
}

public int GetCurrentPlayer()
{
    return currentPlayerID;
}
```

---

### RuleValidator (MonoBehaviour)

**責務**: ルール検証

**メソッド**:
```csharp
public bool CanPlayCard(CardSO card, CardSO fieldCard)
{
    if (card == null) return false;
    if (fieldCard == null) return true; // 場が空なら何でもOK

    return card.GetStrength() > fieldCard.GetStrength();
}

public bool IsCardInHand(CardSO card, PlayerHandSO hand)
{
    return hand.Cards.Contains(card);
}
```

---

### AIPlayer (MonoBehaviour)

**責務**: CPU の決定ロジック

**SerializeField**:
```csharp
[SerializeField] private RuleValidator ruleValidator;
```

**メソッド**:
```csharp
public CardSO DecideAction(PlayerHandSO hand, CardSO fieldCard)
{
    // 1. プレイ可能カード取得
    int fieldStrength = fieldCard?.GetStrength() ?? 0;
    var playableCards = hand.GetPlayableCards(fieldStrength);

    // 2. プレイ可能カードがない → null（パス）
    if (playableCards.Count == 0) return null;

    // 3. 最弱カード選択
    return playableCards
        .OrderBy(c => c.GetStrength())
        .First();
}
```

---

### AIController (MonoBehaviour)

**責務**: CPUターンの実行制御

**SerializeField**:
```csharp
[SerializeField] private float aiTurnDelay = 1.5f;
[SerializeField] private PlayerHandSO[] playerHands;
[SerializeField] private AIPlayer aiPlayer;

[Header("Events - Subscribe")]
[SerializeField] private VoidEventChannelSO onGameStarted;
[SerializeField] private IntEventChannelSO onTurnChanged;
[SerializeField] private CardPlayedEventChannelSO onCardPlayed;
[SerializeField] private VoidEventChannelSO onFieldReset;
[SerializeField] private IntEventChannelSO onGameEnded;

[Header("Events - Raise")]
[SerializeField] private CardEventChannelSO onPlayCardRequested;
[SerializeField] private VoidEventChannelSO onPassButtonClicked;
```

**ランタイム状態**:
```csharp
private CardSO currentFieldCard; // OnCardPlayedイベントから追跡
private bool isGameActive;
```

**イベント購読**:
```csharp
private void OnEnable()
{
    onGameStarted.OnEventRaised += HandleGameStarted;
    onTurnChanged.OnEventRaised += HandleTurnChanged;
    onCardPlayed.OnEventRaised += HandleCardPlayed;
    onFieldReset.OnEventRaised += HandleFieldReset;
    onGameEnded.OnEventRaised += HandleGameEnded;
}

private void OnDisable()
{
    onGameStarted.OnEventRaised -= HandleGameStarted;
    onTurnChanged.OnEventRaised -= HandleTurnChanged;
    onCardPlayed.OnEventRaised -= HandleCardPlayed;
    onFieldReset.OnEventRaised -= HandleFieldReset;
    onGameEnded.OnEventRaised -= HandleGameEnded;
}
```

**主要メソッド**:
```csharp
private void HandleGameStarted()
{
    isGameActive = true;
    currentFieldCard = null;
}

private void HandleTurnChanged(int playerID)
{
    if (playerID >= 1 && playerID <= 3)
    {
        StartCoroutine(ExecuteAITurn(playerID));
    }
}

private void HandleCardPlayed(CardPlayedEventData eventData)
{
    // 場のカードを追跡
    currentFieldCard = eventData.FieldCard;
}

private void HandleFieldReset()
{
    // 場がクリアされた
    currentFieldCard = null;
}

private void HandleGameEnded(int winnerID)
{
    isGameActive = false;
}

private IEnumerator ExecuteAITurn(int aiPlayerID)
{
    yield return new WaitForSeconds(aiTurnDelay);

    // ゲームが終了していたら処理しない
    if (!isGameActive) yield break;

    PlayerHandSO aiHand = playerHands[aiPlayerID];

    // ローカルで追跡している currentFieldCard を使用
    CardSO cardToPlay = aiPlayer.DecideAction(aiHand, currentFieldCard);

    if (cardToPlay != null)
        onPlayCardRequested.RaiseEvent(cardToPlay);
    else
        onPassButtonClicked.RaiseEvent();
}
```

---

## UI層

### GameScreenUI (MonoBehaviour)

**責務**: ゲーム画面全体の制御

**SerializeField**:
```csharp
[Header("Data")]
[SerializeField] private PlayerHandSO[] playerHands;

[Header("Events - Subscribe")]
[SerializeField] private VoidEventChannelSO onGameStarted;
[SerializeField] private IntEventChannelSO onTurnChanged;
[SerializeField] private CardPlayedEventChannelSO onCardPlayed;
[SerializeField] private VoidEventChannelSO onFieldReset;
[SerializeField] private IntEventChannelSO onGameEnded;

[Header("Events - Raise")]
[SerializeField] private CardEventChannelSO onPlayCardRequested;
[SerializeField] private VoidEventChannelSO onPassButtonClicked;
```

**ランタイム状態**:
```csharp
private UIDocument uiDocument;
private VisualElement root;
private HandUI playerHandUI;
private VisualElement fieldCardsContainer;
private VisualElement playerHandContainer;
private Button playCardButton;
private Button passButton;
private Label turnInfoText;
private VisualElement gameEndScreen;
private Label winnerText;
private Button restartButton;

private CardUI currentFieldCardUI;
private int currentPlayerID = -1;
private bool isGameActive = false;
```

**イベント購読**:
```csharp
private void OnEnable()
{
    onGameStarted.OnEventRaised += HandleGameStarted;
    onTurnChanged.OnEventRaised += HandleTurnChanged;
    onCardPlayed.OnEventRaised += HandleCardPlayed;
    onFieldReset.OnEventRaised += HandleFieldReset;
    onGameEnded.OnEventRaised += HandleGameEnded;
}

private void OnDisable()
{
    onGameStarted.OnEventRaised -= HandleGameStarted;
    onTurnChanged.OnEventRaised -= HandleTurnChanged;
    onCardPlayed.OnEventRaised -= HandleCardPlayed;
    onFieldReset.OnEventRaised -= HandleFieldReset;
    onGameEnded.OnEventRaised -= HandleGameEnded;

    if (playerHandUI != null)
        playerHandUI.OnSelectionChanged -= UpdateButtonStates;
}
```

**初期化**:
```csharp
private void Awake()
{
    uiDocument = GetComponent<UIDocument>();
    root = uiDocument.rootVisualElement;

    // UI要素取得
    fieldCardsContainer = root.Q<VisualElement>("FieldCardsContainer");
    playerHandContainer = root.Q<VisualElement>("PlayerHandContainer");
    turnInfoText = root.Q<Label>("TurnInfoText");
    playCardButton = root.Q<Button>("PlayCardButton");
    passButton = root.Q<Button>("PassButton");
    gameEndScreen = root.Q<VisualElement>("GameEndScreen");
    winnerText = root.Q<Label>("WinnerText");
    restartButton = root.Q<Button>("RestartButton");

    // ボタンイベント登録
    playCardButton.clicked += OnPlayCardButtonClick;
    passButton.clicked += OnPassButtonClick;
    restartButton.clicked += OnRestartButtonClick;

    // 初期状態
    playCardButton.SetEnabled(false);
    passButton.SetEnabled(false);
}
```

**主要メソッド**:
```csharp
private void HandleGameStarted()
{
    isGameActive = true;
    currentFieldCardUI = null;
    fieldCardsContainer.Clear();

    // 手札UI初期化
    playerHandUI = new HandUI(playerHandContainer, playerHands[0]);
    playerHandUI.OnSelectionChanged += UpdateButtonStates;
    playerHandUI.Refresh();

    // ゲーム終了画面を非表示
    gameEndScreen.AddToClassList("game-end-screen--hidden");
}

private void HandleTurnChanged(int newPlayerID)
{
    currentPlayerID = newPlayerID;
    UpdateTurnIndicator(newPlayerID);
    UpdateButtonStates();
    UpdatePlayableCardsHighlight();
}

private void HandleCardPlayed(CardPlayedEventData eventData)
{
    // 手札リフレッシュ（プレイヤー0の手札のみ）
    if (eventData.PlayerID == 0)
    {
        playerHandUI.Refresh();
    }

    // 場にカード表示
    DisplayCardOnField(eventData.FieldCard);

    // プレイ可能カードハイライト更新
    UpdatePlayableCardsHighlight();
}

private void HandleFieldReset()
{
    // 場のカードをクリア
    fieldCardsContainer.Clear();
    currentFieldCardUI = null;

    // プレイ可能カードハイライト更新（全カードがプレイ可能）
    UpdatePlayableCardsHighlight();
}

private void HandleGameEnded(int winnerID)
{
    isGameActive = false;
    ShowGameEndScreen(winnerID);
}

private void DisplayCardOnField(CardSO card)
{
    // 既存のカードをクリア
    fieldCardsContainer.Clear();

    if (card != null)
    {
        // 新しいカードを表示
        currentFieldCardUI = new CardUI(card);
        fieldCardsContainer.Add(currentFieldCardUI.Element);
    }
    else
    {
        currentFieldCardUI = null;
    }
}

private void ShowGameEndScreen(int winnerID)
{
    // 勝者メッセージ設定
    if (winnerID == 0)
        winnerText.text = "You Win!";
    else
        winnerText.text = $"CPU {winnerID} Wins!";

    // ゲーム終了画面を表示
    gameEndScreen.RemoveFromClassList("game-end-screen--hidden");

    // ボタンを無効化
    playCardButton.SetEnabled(false);
    passButton.SetEnabled(false);
}

private void UpdateTurnIndicator(int playerID)
{
    if (playerID == 0)
        turnInfoText.text = "Your Turn";
    else
        turnInfoText.text = $"CPU {playerID}'s Turn";
}

private void UpdateButtonStates()
{
    bool isPlayerTurn = (currentPlayerID == 0 && isGameActive);

    // Play Cardボタン：プレイヤーのターン AND カード選択済み
    var selectedCards = playerHandUI?.GetSelectedCards();
    playCardButton.SetEnabled(isPlayerTurn && selectedCards != null && selectedCards.Count > 0);

    // Passボタン：プレイヤーのターンのみ
    passButton.SetEnabled(isPlayerTurn);
}

private void UpdatePlayableCardsHighlight()
{
    if (currentPlayerID != 0 || playerHandUI == null) return; // 人間ターンのみ

    int fieldStrength = currentFieldCardUI?.CardData?.GetStrength() ?? 0;
    var playableCards = playerHands[0].GetPlayableCards(fieldStrength);
    playerHandUI.HighlightPlayableCards(playableCards);
}

private void OnPlayCardButtonClick()
{
    var selectedCards = playerHandUI.GetSelectedCards();
    if (selectedCards.Count == 0) return;

    // カードプレイリクエスト発行
    onPlayCardRequested.RaiseEvent(selectedCards[0]);

    // 選択解除
    playerHandUI.ClearSelection();
}

private void OnPassButtonClick()
{
    onPassButtonClicked.RaiseEvent();
}

private void OnRestartButtonClick()
{
    UnityEngine.SceneManagement.SceneManager.LoadScene(
        UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
    );
}
```

---

### HandUI (C# class, NOT MonoBehaviour)

**責務**: プレイヤー手札の表示と選択管理

**プロパティ**:
```csharp
private readonly VisualElement handContainer;
private readonly PlayerHandSO handData;
private readonly CardEventChannelSO onCardPlayedEvent;
private readonly List<CardUI> cardUIElements = new();
private CardUI selectedCard = null;
private List<CardSO> playableCards = new();

public event Action OnSelectionChanged;
```

**メソッド**:
```csharp
public void Refresh()
{
    handContainer.Clear();
    cardUIElements.Clear();

    foreach (var card in handData.Cards)
    {
        CardUI cardUI = new CardUI(card);
        cardUIElements.Add(cardUI);
        handContainer.Add(cardUI.Element);

        if (handData.PlayerID == 0) // 人間プレイヤーのみ
            cardUI.Element.RegisterCallback<ClickEvent>(evt => OnCardClicked(cardUI));
    }

    // 選択状態をクリア
    selectedCard = null;
}

private void OnCardClicked(CardUI cardUI)
{
    // Phase 1: 1枚のみ選択
    if (!playableCards.Contains(cardUI.CardData)) return; // プレイ不可

    if (selectedCard == cardUI)
    {
        // 選択解除
        selectedCard.SetSelected(false);
        selectedCard = null;
        OnSelectionChanged?.Invoke(); // 選択解除時も発火
    }
    else
    {
        // 前の選択解除
        selectedCard?.SetSelected(false);

        // 新規選択
        selectedCard = cardUI;
        selectedCard.SetSelected(true);
        OnSelectionChanged?.Invoke(); // 選択時も発火
    }
}

public List<CardSO> GetSelectedCards()
{
    return selectedCard != null ? new List<CardSO> { selectedCard.CardData } : new List<CardSO>();
}

public void ClearSelection()
{
    if (selectedCard != null)
    {
        selectedCard.SetSelected(false);
        selectedCard = null;
        OnSelectionChanged?.Invoke();
    }
}

public void HighlightPlayableCards(List<CardSO> newPlayableCards)
{
    playableCards = newPlayableCards;

    foreach (var cardUI in cardUIElements)
    {
        if (playableCards.Contains(cardUI.CardData))
            cardUI.AddClass("card--playable");
        else
            cardUI.RemoveClass("card--playable");
    }
}
```

---

### CardUI (C# class, NOT MonoBehaviour)

**責務**: 1枚のカードのUI表現

**プロパティ**:
```csharp
private readonly VisualElement element;
private readonly CardSO cardData;

public VisualElement Element => element;
public CardSO CardData => cardData;
```

**メソッド**:
```csharp
public CardUI(CardSO card)
{
    cardData = card;
    element = new VisualElement();
    element.AddToClassList("card");

    var image = new VisualElement();
    image.AddToClassList("card__image");
    image.style.backgroundImage = new StyleBackground(card.CardSprite);
    element.Add(image);
}

public void SetSelected(bool selected)
{
    if (selected)
        element.AddToClassList("card--selected");
    else
        element.RemoveFromClassList("card--selected");
}

public void AddClass(string className)
{
    element.AddToClassList(className);
}

public void RemoveClass(string className)
{
    element.RemoveFromClassList(className);
}
```

---

## UXML/USS 構造

### GameScreen.uxml

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <Style src="../USS/Common.uss" />
    <Style src="../USS/GameScreen.uss" />
    <Style src="../USS/Card.uss" />

    <ui:VisualElement name="Root" class="game-screen">
        <!-- ターン表示 -->
        <ui:Label name="TurnInfoText" class="turn-info" text="Your Turn" />

        <!-- 場のカード -->
        <ui:VisualElement name="FieldCardsContainer" class="field-cards" />

        <!-- プレイヤー手札 -->
        <ui:VisualElement name="PlayerHandContainer" class="player-hand" />

        <!-- ボタン -->
        <ui:VisualElement class="player-actions">
            <ui:Button name="PlayCardButton" text="Play Card" class="button-primary" />
            <ui:Button name="PassButton" text="Pass" class="button-secondary" />
        </ui:VisualElement>

        <!-- ゲーム終了画面 -->
        <ui:VisualElement name="GameEndScreen" class="game-end-screen game-end-screen--hidden">
            <ui:Label name="WinnerText" class="winner-text" />
            <ui:Button name="RestartButton" text="Restart" class="button-primary" />
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

### Card.uss

```css
.card {
    width: 100px;
    height: 136px;
    border-radius: 8px;
    margin: 4px;
    transition-property: transform;
    transition-duration: 0.2s;
}

.card--selected {
    transform: translateY(-20px);
}

.card--playable {
    border-color: #00ff00;
    border-width: 3px;
}

.card__image {
    width: 100%;
    height: 100%;
    -unity-background-scale-mode: scale-and-crop;
}
```

---

## 実装順序

### フェーズ1: データ層
1. CardSO 実装
2. DeckSO 実装
3. PlayerHandSO 実装
4. 52枚のCardSOアセット作成

### フェーズ2: EventChannel層
1. EventChannel SOアセット作成
2. Variables SOアセット作成

### フェーズ3: ゲームロジック層
1. RuleValidator 実装
2. TurnManager 実装
3. GameManager 実装（カード配布まで）
4. AIPlayer 実装
5. AIController 実装

### フェーズ4: UI層（最小限）
1. UXML/USS作成
2. CardUI 実装
3. HandUI 実装
4. GameScreenUI 実装（基本機能のみ）

### フェーズ5: 統合とテスト
1. 全コンポーネントを接続
2. Inspector設定
3. プレイテスト
4. バグ修正

### フェーズ6: UI強化
1. ハイライト機能
2. ボタン状態管理
3. ゲーム終了画面
4. 最終調整

---

## 次のステップ

この実装計画を基に、以下を実行する：

- **/tasks**: 実装タスクへの詳細な分解

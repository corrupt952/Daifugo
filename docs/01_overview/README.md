# Daifugo - Project Overview

## Purpose

This document provides a comprehensive overview of the Daifugo (大富豪) card game learning project. It outlines the project goals, technical approach, architecture patterns, implementation phases, and learning objectives for building a traditional Japanese card game using Unity.

## Checklist

- [ ] Understand the two-phase development approach (2D → 3D)
- [ ] Review ScriptableObject-driven architecture patterns
- [ ] Study UI Toolkit implementation for Phase 1
- [ ] Plan Phase 2 3D enhancements with LitMotion
- [ ] Follow SpecKit workflow for implementation

---

## Project Description

**Daifugo (大富豪)** is a learning project that implements a traditional Japanese card game using Unity. The project follows a phased approach: **Phase 1 (2D with UI Toolkit) → Phase 2 (3D with enhanced visuals)**, enabling progressive learning of different technology stacks.

### Learning Objectives

This project aims to develop proficiency in the following areas:

1. **Deep Understanding of UI Toolkit**
   - Drag & drop interactions (PointerManipulator)
   - Dynamic list display (ListView)
   - USS animations
   - Complex layouts (Flexbox)

2. **2D Development Experience**
   - Current state: Primarily 3D project experience
   - Weakness: Limited 2D game development (only falling puzzle game)
   - Goal: Strengthen 2D development fundamentals through card game implementation

3. **Practical Application of Established Architecture**
   - ScriptableObject-driven design
   - EventChannel pattern (Tang3cko)
   - RuntimeSet pattern
   - BEM naming conventions (USS)
   - Apply Rookie project standards to a new project

4. **3D Visual Enhancement (Phase 2)**
   - Card placement in 3D space
   - Camera work (Cinemachine)
   - High-performance animation (LitMotion)
   - Particle effects and lighting

---

## Development Phases

### Phase 1: 2D Version (UI Toolkit Foundation)

**Duration:** 1-2 weeks

**Technology Stack:**
- Unity 2D or 3D Template
- UI Toolkit (100%)
- Tang3cko.EventChannels
- ScriptableObject architecture

**Implementation Scope:**
- Game logic (Daifugo rules)
- Card display with UI Toolkit
- Drag & drop interactions
- Turn management and score display
- AI opponent (simple rule-based)

**Learning Focus:**
- UI Toolkit PointerManipulator
- Card images as background images
- USS transitions for animations
- EventChannel-driven game flow

---

### Phase 2: 3D Version (Visual Enhancement)

**Duration:** 1 week

**Technology Stack:**
- Unity 3D URP
- UI Toolkit (HUD and scores only)
- LitMotion (card animations)
- Cinemachine (camera work)
- Particle System (visual effects)

**Implementation Scope:**
- **Reuse Phase 1 game logic**
- 3D card placement in space (hand displayed in fan formation)
- Card rotation animations
- Camera perspective switching (overhead ↔ close-up)
- Particle effects (when cards are played)
- Lighting (card material appearance)

**Learning Focus:**
- 2D → 3D logic migration
- 3D UI approaches (World Space Canvas vs UI Toolkit)
- Rich visual effects implementation

---

## Architecture Design

### Design Principles

1. **ScriptableObject-Driven** - Separation of data and logic
2. **Event-Driven Communication** - Decoupling via Tang3cko.EventChannels
3. **RuntimeSet Pattern** - Dynamic object management
4. **Single Responsibility Principle** - Component responsibility separation
5. **2D→3D Logic Reuse** - Phase 1 core logic reused in Phase 2

### Directory Structure

```
Daifugo/
├── Assets/
│   ├── _Project/
│   │   ├── ScriptableObjects/
│   │   │   ├── Cards/          # CardSO (52 cards)
│   │   │   ├── Data/           # DeckSO, PlayerHandSO
│   │   │   └── EventChannels/  # Event definitions
│   │   │
│   │   ├── Scripts/
│   │   │   ├── Core/           # Game logic (2D/3D shared)
│   │   │   ├── UI/             # UI layer (2D/3D variants)
│   │   │   │   ├── 2D/         # Phase 1
│   │   │   │   └── 3D/         # Phase 2
│   │   │   └── Data/           # Data classes
│   │   │
│   │   ├── UI/
│   │   │   ├── UXML/           # UI structure definitions
│   │   │   └── USS/            # Style definitions
│   │   │
│   │   └── Art/
│   │       ├── Cards/          # Card images
│   │       └── UI/             # UI assets
│   │
│   └── Packages/
│       └── Tang3cko.EventChannels
```

---

## ScriptableObject Architecture

### CardSO (Card Data)

```csharp
using UnityEngine;

namespace Daifugo.Data
{
    /// <summary>
    /// Immutable card data
    /// </summary>
    [CreateAssetMenu(fileName = "Card", menuName = "Daifugo/Data/Card")]
    public class CardSO : ScriptableObject
    {
        [Header("Card Properties")]
        public Suit suit;           // Spades, Hearts, Diamonds, Clubs
        public int rank;            // 1-13 (1=Ace, 11=Jack, 12=Queen, 13=King)

        [Header("Visual")]
        public Sprite cardSprite;   // Used in 2D version
        public Material cardMaterial; // Used in 3D version (Phase 2)

        /// <summary>
        /// Gets the card strength according to Daifugo rules
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

### DeckSO (Deck Management)

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace Daifugo.Data
{
    /// <summary>
    /// Runtime deck management
    /// </summary>
    [CreateAssetMenu(fileName = "Deck", menuName = "Daifugo/Data/Deck")]
    public class DeckSO : ScriptableObject
    {
        [SerializeField] private List<CardSO> allCards; // 52 cards
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

### PlayerHandSO (Hand Management)

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace Daifugo.Data
{
    /// <summary>
    /// Player hand management
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerHand", menuName = "Daifugo/Data/PlayerHand")]
    public class PlayerHandSO : ScriptableObject
    {
        private List<CardSO> hand = new List<CardSO>();

        public IReadOnlyList<CardSO> Cards => hand;

        public void AddCard(CardSO card)
        {
            hand.Add(card);
            SortHand();
        }

        public void RemoveCard(CardSO card)
        {
            hand.Remove(card);
        }

        public void Clear()
        {
            hand.Clear();
        }

        private void SortHand()
        {
            hand.Sort((a, b) => a.GetStrength().CompareTo(b.GetStrength()));
        }

        public bool HasCard(CardSO card) => hand.Contains(card);
        public int CardCount => hand.Count;
    }
}
```

---

## EventChannel Architecture

### CardEventChannelSO

```csharp
using Tang3cko.EventChannels;
using UnityEngine;

namespace Daifugo.Events
{
    /// <summary>
    /// Event channel for card-related events
    /// </summary>
    [CreateAssetMenu(fileName = "CardEventChannel", menuName = "Daifugo/Events/Card Event Channel")]
    public class CardEventChannelSO : EventChannelSO<CardSO>
    {
        // Tang3cko.EventChannels base class provides all functionality
    }
}
```

### TurnEventChannelSO

```csharp
using Tang3cko.EventChannels;
using UnityEngine;

namespace Daifugo.Events
{
    /// <summary>
    /// Event channel for turn changes
    /// </summary>
    [CreateAssetMenu(fileName = "TurnEventChannel", menuName = "Daifugo/Events/Turn Event Channel")]
    public class TurnEventChannelSO : EventChannelSO<int>
    {
        // int = Player ID (0-3)
    }
}
```

---

## UI Toolkit Implementation (2D Version)

### UXML Structure Example

```xml
<!-- GameScreen.uxml -->
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <Style src="Common.uss" />
    <Style src="GameUI.uss" />

    <!-- Game container -->
    <ui:VisualElement class="game-screen">

        <!-- Opponent hand area (back side visible) -->
        <ui:VisualElement class="opponent-hand-area">
            <ui:Label name="OpponentNameText" text="CPU" class="player-name" />
            <ui:VisualElement name="OpponentHandContainer" class="hand-container" />
        </ui:VisualElement>

        <!-- Field area -->
        <ui:VisualElement class="field-area">
            <ui:Label name="TurnInfoText" text="Your Turn" class="turn-info" />
            <ui:VisualElement name="FieldCardsContainer" class="field-cards" />
        </ui:VisualElement>

        <!-- Player hand area -->
        <ui:VisualElement class="player-hand-area">
            <ui:Label name="PlayerNameText" text="You" class="player-name" />
            <ui:VisualElement name="PlayerHandContainer" class="hand-container" />
            <ui:Button name="PassButton" text="Pass" class="button-primary" />
        </ui:VisualElement>

        <!-- Score panel -->
        <ui:VisualElement class="score-panel">
            <ui:Label name="ScoreText" text="Score: 0" class="score-text" />
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

### USS Style Example

```css
/* Common.uss */
:root {
    --color-primary: #FFD700;
    --color-card-bg: rgba(255, 255, 255, 0.9);
    --color-field-bg: rgba(34, 139, 34, 0.8);
    --font-size-large: 24px;
    --font-size-medium: 18px;
}

/* GameUI.uss */
.game-screen {
    width: 100%;
    height: 100%;
    flex-direction: column;
    justify-content: space-between;
    background-color: var(--color-field-bg);
}

.hand-container {
    flex-direction: row;
    justify-content: center;
    padding: 16px;
}

.card {
    width: 80px;
    height: 120px;
    border-radius: 8px;
    background-color: var(--color-card-bg);
    margin: 4px;
    transition: transform 0.3s;
}

.card:hover {
    transform: scale(1.1) translateY(-20px);
}

.field-area {
    flex-grow: 1;
    align-items: center;
    justify-content: center;
}

.field-cards {
    flex-direction: row;
    flex-wrap: wrap;
}

.turn-info {
    font-size: var(--font-size-large);
    color: var(--color-primary);
    -unity-font-style: bold;
}
```

### CardDragManipulator (Drag & Drop)

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using Tang3cko.EventChannels;

namespace Daifugo.UI
{
    /// <summary>
    /// Handles card drag and drop interactions
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

                // Check drop position
                if (IsOverPlayArea(evt.position))
                {
                    // Raise EventChannel
                    onCardPlayed?.RaiseEvent(cardData);
                }
                else
                {
                    // Return to original position
                    target.transform.position = startPosition;
                }
            }
        }

        private bool IsOverPlayArea(Vector2 position)
        {
            // Check if over field area
            VisualElement fieldArea = target.panel.visualTree.Q<VisualElement>("FieldCardsContainer");
            if (fieldArea == null) return false;

            return fieldArea.worldBound.Contains(position);
        }
    }
}
```

---

## Game Logic

### GameManager (Overall Game Management)

```csharp
using UnityEngine;
using Tang3cko.EventChannels;

namespace Daifugo.Core
{
    /// <summary>
    /// Manages the overall Daifugo game
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private DeckSO deck;
        [SerializeField] private PlayerHandSO playerHand;
        [SerializeField] private PlayerHandSO[] aiHands;

        [Header("Event Channels - Input")]
        [SerializeField] private CardEventChannelSO onCardPlayed;
        [SerializeField] private VoidEventChannelSO onPassButtonClicked;

        [Header("Event Channels - Output")]
        [SerializeField] private VoidEventChannelSO onGameStarted;
        [SerializeField] private TurnEventChannelSO onTurnChanged;
        [SerializeField] private IntEventChannelSO onGameEnded; // winner player ID

        private TurnManager turnManager;
        private RuleValidator ruleValidator;

        private void Awake()
        {
            turnManager = new TurnManager(4); // 4 players
            ruleValidator = new RuleValidator();
        }

        private void Start()
        {
            StartGame();
        }

        private void OnEnable()
        {
            onCardPlayed.OnEventRaised += HandleCardPlayed;
            onPassButtonClicked.OnEventRaised += HandlePass;
        }

        private void OnDisable()
        {
            onCardPlayed.OnEventRaised -= HandleCardPlayed;
            onPassButtonClicked.OnEventRaised -= HandlePass;
        }

        private void StartGame()
        {
            // Initialize deck
            deck.Initialize();

            // Distribute cards
            DistributeCards();

            onGameStarted?.RaiseEvent();
            onTurnChanged?.RaiseEvent(turnManager.CurrentPlayerID);
        }

        private void DistributeCards()
        {
            playerHand.Clear();
            foreach (var hand in aiHands)
                hand.Clear();

            // Distribute all cards
            while (deck.RemainingCards > 0)
            {
                playerHand.AddCard(deck.DrawCard());
                foreach (var aiHand in aiHands)
                {
                    if (deck.RemainingCards > 0)
                        aiHand.AddCard(deck.DrawCard());
                }
            }
        }

        private void HandleCardPlayed(CardSO card)
        {
            // Rule validation
            if (!ruleValidator.CanPlayCard(card))
            {
                Debug.Log("Cannot play this card!");
                return;
            }

            // Remove card
            playerHand.RemoveCard(card);

            // Check win condition
            if (playerHand.CardCount == 0)
            {
                onGameEnded?.RaiseEvent(0); // Player wins
                return;
            }

            // Advance turn
            turnManager.NextTurn();
            onTurnChanged?.RaiseEvent(turnManager.CurrentPlayerID);
        }

        private void HandlePass()
        {
            turnManager.NextTurn();
            onTurnChanged?.RaiseEvent(turnManager.CurrentPlayerID);
        }
    }
}
```

---

## AI Implementation (Simple Version)

```csharp
namespace Daifugo.Core
{
    /// <summary>
    /// Simple rule-based AI
    /// </summary>
    public class AIPlayer
    {
        private PlayerHandSO hand;

        public AIPlayer(PlayerHandSO handData)
        {
            hand = handData;
        }

        public CardSO DecideCard(RuleValidator rules)
        {
            // Choose the weakest playable card
            foreach (var card in hand.Cards)
            {
                if (rules.CanPlayCard(card))
                    return card;
            }
            return null; // Pass
        }
    }
}
```

---

## 3D Version Visual Enhancement Ideas

### Camera Work

- **Your turn:** Focus on hand (close-up)
- **Opponent turn:** Overhead view
- **Card play:** Slow motion + camera follow

### Animations

```csharp
// Card rotation with LitMotion
LMotion.Create(card.transform.rotation, Quaternion.Euler(0, 180, 0), 0.5f)
    .WithEase(Ease.OutBack)
    .Bind(x => card.transform.rotation = x);

// Card flies to field
LMotion.Create(card.transform.position, fieldPosition, 0.8f)
    .WithEase(Ease.OutQuad)
    .Bind(x => card.transform.position = x);
```

### Particle Effects

- **Card play:** Sparkle effect
- **Victory:** Confetti

### Lighting

- **Directional Light:** Main light
- **Point Light:** Spotlight on cards (during your turn)

---

## Game Rules

### Basic Daifugo Rules

1. **Objective:** First player to empty their hand wins
2. **Card Strength:** 2 > A > K > Q > ... > 3
3. **Play Methods:**
   - Play cards stronger than those on the field
   - Can play multiple cards of the same rank simultaneously
   - Can pass (field resets when all players pass)

### Phase 1 Implementation

- Basic play mechanics
- Turn management
- Win/loss determination
- Simple AI (plays weakest card)

### Extended Rules (Phase 2+)

- Revolution (playing 4 cards reverses strength)
- Eight cut (playing an 8 resets the field)
- Sequence (3+ consecutive ranks)
- Jokers

---

## Learning Resources

### Official Documentation

- **UI Toolkit:** https://docs.unity3d.com/Manual/UIElements.html
- **ScriptableObject:** https://docs.unity3d.com/Manual/class-ScriptableObject.html
- **LitMotion:** https://github.com/AnnulusGames/LitMotion
- **Cinemachine:** https://docs.unity3d.com/Packages/com.unity.cinemachine@latest

### Card Assets

- **Kenney Card Pack:** https://kenney.nl/assets/playing-cards-pack
- **OpenGameArt:** https://opengameart.org/

---

## Implementation Checklists

### Phase 1: 2D Version

- [ ] Project setup (Unity 2D Template)
- [ ] Integrate Tang3cko.EventChannels
- [ ] Design ScriptableObjects (CardSO, DeckSO, PlayerHandSO)
- [ ] Create UI Toolkit UXML/USS
- [ ] Implement drag & drop (CardDragManipulator)
- [ ] Implement game logic (GameManager, TurnManager)
- [ ] Implement rule validation (RuleValidator)
- [ ] Implement AI (simple rule-based)
- [ ] Create score display and turn management UI
- [ ] Complete game flow (deal → play → win/loss determination)

### Phase 2: 3D Version

- [ ] Create new project with 3D URP Template
- [ ] Port Phase 1 logic (GameManager, etc.)
- [ ] Place 3D card models
- [ ] Implement camera work (Cinemachine)
- [ ] Implement animations with LitMotion
- [ ] Add particle effects
- [ ] Adjust lighting
- [ ] Implement UI Toolkit (HUD only)

---

## Next Steps

1. **Phase 1 Detailed Design** (SpecKit Workflow)
   - Use `/specify` to create Phase 1 specifications
   - Use `/clarify` to resolve ambiguities
   - Use `/plan` to establish implementation plan
   - Use `/tasks` to break down tasks

2. **Project Setup**
   - Integrate Tang3cko.EventChannels
   - Set up directory structure
   - Prepare card assets

3. **Begin Implementation**
   - Create ScriptableObjects (CardSO, DeckSO)
   - Implement UI Toolkit UXML/USS
   - Implement game logic

---

## Future Expansion Ideas

### Phase 3 and Beyond

- **Online Multiplayer** (Mirror Networking)
  - Leverage Rookie project knowledge
  - Lobby and matchmaking features

- **Rule Variations**
  - Revolution, eight cut, sequences
  - Custom rule settings

- **Ranking and Achievement System**
  - Steam achievement integration (after Phase 2 completion)

- **Mobile Support**
  - Touch input optimization
  - Responsive UI

---

## Technical Decision Summary

### Why UI Toolkit?

- **Latest technology:** Unity's next-generation UI system
- **High performance:** CSS-like styling
- **Learning value:** Essential skill for future Unity development
- **2D suitability:** Ideal for 2D card games

### Why ScriptableObject?

- **Data-driven:** Manage 52 cards as assets
- **Hot reload:** Modify values during runtime
- **Memory efficient:** Share same data via references
- **Testability:** Separation of logic and data

### Why EventChannel?

- **Decoupling:** Separation of UI and logic layers
- **Extensibility:** Easy to add new features
- **Debugging:** Easy to visualize event flow
- **Proven:** Established in Rookie project

### Why LitMotion (Not DOTween)?

- **Best performance:** Approximately 5x faster than DOTween, zero allocation
- **Latest technology:** Leverages Unity DOTS (C# Job System + Burst Compiler)
- **Rich features:** v2 supports Sequence functionality and Inspector editing
- **Learning value:** Opportunity to learn latest Unity technology

---

## References

- [SpecKit - Spec-Driven Development](https://github.com/github/spec-kit)
- [Coding Standards](../03_technical/coding_standards.md)
- [Design Decisions](../00_spec/clarifications.md)
- [Unity UI Toolkit Documentation](https://docs.unity3d.com/Manual/UIElements.html)
- [Tang3cko.EventChannels Package](https://github.com/Tang3cko/EventChannels)
- [LitMotion GitHub](https://github.com/AnnulusGames/LitMotion)
- [Cinemachine Documentation](https://docs.unity3d.com/Packages/com.unity.cinemachine@latest)

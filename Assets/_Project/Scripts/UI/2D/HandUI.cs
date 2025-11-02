using System;
using System.Collections.Generic;
using System.Linq;
using Daifugo.Data;
using Daifugo.Events;
using UnityEngine;
using UnityEngine.UIElements;

namespace Daifugo.UI
{
    /// <summary>
    /// UI controller for a player's hand display
    /// </summary>
    public class HandUI
    {
        private readonly VisualElement handContainer;
        private readonly PlayerHandSO handData;
        private readonly List<CardUI> cardUIElements = new();
        private readonly List<CardUI> selectedCards = new(); // Phase 1.5: Multiple card selection
        private List<CardSO> playableCards = new();

        /// <summary>
        /// Event raised when card selection changes
        /// </summary>
        public event Action OnSelectionChanged;

        /// <summary>
        /// Creates a new HandUI instance
        /// Phase 1: Only supports player hand (ID 0) with single card selection
        /// </summary>
        public HandUI(VisualElement container, PlayerHandSO hand)
        {
            if (container == null)
            {
                Debug.LogError("[HandUI] Cannot create HandUI with null container.");
                return;
            }

            if (hand == null)
            {
                Debug.LogError("[HandUI] Cannot create HandUI with null PlayerHandSO.");
                return;
            }

            handContainer = container;
            handData = hand;

            Refresh();
        }

        /// <summary>
        /// Refreshes the hand display to match current hand data
        /// </summary>
        public void Refresh()
        {
            // Clear existing UI
            handContainer.Clear();
            cardUIElements.Clear();

            // Create CardUI for each card in hand
            foreach (var card in handData.Cards)
            {
                CardUI cardUI = new CardUI(card);
                cardUIElements.Add(cardUI);
                handContainer.Add(cardUI.Element);

                // Add click handler for player hand (Player 0)
                if (handData.PlayerID == 0)
                {
                    cardUI.Element.RegisterCallback<ClickEvent>(evt => OnCardClicked(cardUI));
                }
            }
        }

        /// <summary>
        /// Gets the CardUI for a specific CardSO
        /// </summary>
        public CardUI GetCardUI(CardSO card)
        {
            return cardUIElements.Find(cardUI => cardUI.CardData == card);
        }

        /// <summary>
        /// Gets all CardUI elements
        /// </summary>
        public List<CardUI> GetAllCardUIs()
        {
            return new List<CardUI>(cardUIElements);
        }

        /// <summary>
        /// Removes a CardUI from the display
        /// </summary>
        public void RemoveCard(CardSO card)
        {
            CardUI cardUI = GetCardUI(card);
            if (cardUI != null)
            {
                handContainer.Remove(cardUI.Element);
                cardUIElements.Remove(cardUI);
            }
        }

        /// <summary>
        /// Enables or disables all card interactions
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            foreach (var cardUI in cardUIElements)
            {
                cardUI.SetDisabled(!interactable);
            }
        }

        /// <summary>
        /// Handles card click event
        /// Phase 1.5: Multiple card selection, only playable cards can be selected
        /// </summary>
        private void OnCardClicked(CardUI cardUI)
        {
            // Check if card is playable
            if (!playableCards.Contains(cardUI.CardData))
            {
                return; // Cannot select non-playable cards
            }

            // Toggle selection
            if (selectedCards.Contains(cardUI))
            {
                // Deselect card
                cardUI.SetSelected(false);
                selectedCards.Remove(cardUI);
                OnSelectionChanged?.Invoke();
            }
            else
            {
                // Select card
                cardUI.SetSelected(true);
                selectedCards.Add(cardUI);
                OnSelectionChanged?.Invoke();
            }
        }

        /// <summary>
        /// Gets currently selected cards (Phase 1.5: Returns list of all selected cards)
        /// </summary>
        public List<CardSO> GetSelectedCards()
        {
            return selectedCards.Select(cardUI => cardUI.CardData).ToList();
        }

        /// <summary>
        /// Clears card selection (Phase 1.5: Clears all selected cards)
        /// </summary>
        public void ClearSelection()
        {
            if (selectedCards.Count > 0)
            {
                foreach (var cardUI in selectedCards)
                {
                    cardUI.SetSelected(false);
                }
                selectedCards.Clear();
                OnSelectionChanged?.Invoke();
            }
        }

        /// <summary>
        /// Highlights playable cards
        /// </summary>
        public void HighlightPlayableCards(List<CardSO> newPlayableCards)
        {
            // Store playable cards for selection validation
            playableCards = newPlayableCards;

            foreach (var cardUI in cardUIElements)
            {
                if (playableCards.Contains(cardUI.CardData))
                {
                    cardUI.AddClass("card--playable");
                }
                else
                {
                    cardUI.RemoveClass("card--playable");
                }
            }
        }
    }
}

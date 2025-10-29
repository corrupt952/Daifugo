using Daifugo.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace Daifugo.UI
{
    /// <summary>
    /// UI representation of a single card
    /// </summary>
    public class CardUI
    {
        private readonly VisualElement cardElement;
        private readonly CardSO cardData;

        /// <summary>
        /// Gets the card data
        /// </summary>
        public CardSO CardData => cardData;

        /// <summary>
        /// Gets the visual element
        /// </summary>
        public VisualElement Element => cardElement;

        /// <summary>
        /// Creates a new CardUI instance
        /// </summary>
        public CardUI(CardSO card)
        {
            if (card == null)
            {
                Debug.LogError("[CardUI] Cannot create CardUI with null CardSO.");
                return;
            }

            cardData = card;

            // Create card visual element
            cardElement = new VisualElement();
            cardElement.AddToClassList("card");
            cardElement.pickingMode = PickingMode.Position; // Enable pointer events

            // Create card image
            VisualElement cardImage = new VisualElement();
            cardImage.AddToClassList("card__image");
            cardImage.pickingMode = PickingMode.Ignore; // Let parent handle events

            // Set card sprite as background image
            if (card.CardSprite != null)
            {
                cardImage.style.backgroundImage = new StyleBackground(card.CardSprite);
            }
            else
            {
                Debug.LogWarning($"[CardUI] Card {card.CardSuit} {card.Rank} has no sprite assigned.");
            }

            cardElement.Add(cardImage);

            // Store card data reference in user data
            cardElement.userData = card;
        }

        /// <summary>
        /// Adds a CSS class to the card element
        /// </summary>
        public void AddClass(string className)
        {
            cardElement?.AddToClassList(className);
        }

        /// <summary>
        /// Removes a CSS class from the card element
        /// </summary>
        public void RemoveClass(string className)
        {
            cardElement?.RemoveFromClassList(className);
        }

        /// <summary>
        /// Toggles a CSS class on the card element
        /// </summary>
        public void ToggleClass(string className)
        {
            cardElement?.ToggleInClassList(className);
        }

        /// <summary>
        /// Sets the selected state of the card
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (selected)
            {
                AddClass("card--selected");
            }
            else
            {
                RemoveClass("card--selected");
            }
        }

        /// <summary>
        /// Sets the dragging state of the card
        /// </summary>
        public void SetDragging(bool dragging)
        {
            if (dragging)
            {
                AddClass("card--dragging");
            }
            else
            {
                RemoveClass("card--dragging");
            }
        }

        /// <summary>
        /// Sets the disabled state of the card
        /// </summary>
        public void SetDisabled(bool disabled)
        {
            if (disabled)
            {
                AddClass("card--disabled");
            }
            else
            {
                RemoveClass("card--disabled");
            }
        }
    }
}

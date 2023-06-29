using SimpleSolitaire.Model.Enum;
using System.Collections.Generic;
using System.Linq;
using SimpleSolitaire.Model.Config;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    public class FreecellDeck : Deck
    {
        /// <summary>
        /// Update card position in game by solitaire style
        /// </summary>
        /// <param name="firstTime">If it first game update</param>
        public override void UpdateCardsPosition(bool firstTime)
        {
            for (int i = 0; i < CardsArray.Count; i++)
            {
                Card card = CardsArray[i];
                card.transform.SetAsLastSibling();

                if (Type == DeckType.DECK_TYPE_ACE)
                {
                    card.gameObject.transform.position = gameObject.transform.position;
                }
                else if (Type == DeckType.DECK_TYPE_FREECELL)
                {
                    card.gameObject.transform.position = gameObject.transform.position;
                }
                
                card.UpdateCardImg();
            }

            if (Type == DeckType.DECK_TYPE_BOTTOM)
            {
                UpdateDraggableStatus();

                for (int i = 0; i < CardsArray.Count; i++)
                {
                    Card card = CardsArray[i];
                    Card prevCard = i > 0 ? CardsArray[i - 1] : null;

                    var space = prevCard != null && prevCard.CardStatus == 1 ? 
                        CardLogicComponent.GetSpaceFromDictionary(DeckSpacesTypes.DECK_SPACE_VERTICAL_BOTTOM_OPENED) :
                        CardLogicComponent.GetSpaceFromDictionary(DeckSpacesTypes.DECK_SPACE_VERTICAL_BOTTOM_CLOSED);
                    var spaceMultiplier = prevCard != null ? 1 : 0;
                    var deckPos = gameObject.transform.position;
                    var prevPos = prevCard != null ? prevCard.gameObject.transform.position : deckPos;

                    var curPos = prevPos - new Vector3(0, space, 0) * spaceMultiplier;
                    card.gameObject.transform.position = curPos;
                }
            }

            UpdateCardsActiveStatus();
        }

        /// <summary>
        /// If we can drop card to other card it will be true.
        /// </summary>
        /// <param name="card">Checking card</param>
        /// <returns>We can drop or no</returns>
        public override bool AcceptCard(Card card)
        {
            Card topCard = GetTopCard();
            switch (Type)
            {
                case DeckType.DECK_TYPE_FREECELL:
                {
                    return topCard == null;
                }

                case DeckType.DECK_TYPE_BOTTOM:
                {
                    if (topCard != null)
                    {
                        return topCard.CardColor != card.CardColor && topCard.Number == card.Number + 1;
                    }
                    else
                    {
                        return true;
                    }
                }
                case DeckType.DECK_TYPE_ACE:
                {
                    Deck srcDeck = card.Deck;
                    if (srcDeck.GetTopCard() != card)
                    {
                        return false;
                    }

                    if (topCard != null)
                    {
                        if (topCard.CardType == card.CardType && topCard.Number == card.Number - 1)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (card.Number == 1)
                        {
                            return true;
                        }

                        return false;
                    }
                }
            }

            return false;
        }

        public override void UpdateDraggableStatus()
        {
            if (Type == DeckType.DECK_TYPE_BOTTOM)
            {
                if (CardsCount == 0)
                {
                    return;
                }

                Card topCard = CardsArray[CardsArray.Count - 1];
                int topNumber = topCard.Number;
                int nextColor = topCard.CardColor == 0 ? 1 : 0;
                bool isDraggable = true;
                topCard.IsDraggable = isDraggable;

                for (int i = CardsArray.Count - 2; i >= 0; i--)
                {
                    var card = CardsArray[i];
                    int nextNumber = card.Number;

                    if (isDraggable && card.CardStatus == 1 && nextNumber == topNumber + 1 &&
                        card.CardColor == nextColor)
                    {
                        topNumber++;
                        nextColor = card.CardColor == 0 ? 1 : 0;
                    }
                    else
                    {
                        isDraggable = false;
                    }

                    card.IsDraggable = isDraggable;
                }

                UpdateBackgroundColor();
            }
            else if (Type == DeckType.DECK_TYPE_FREECELL)
            {
                for (int i = 0; i < CardsArray.Count; i++)
                {
                    Card card = CardsArray[i];
                    card.IsDraggable = true;
                }
            }
            else if (Type == DeckType.DECK_TYPE_ACE)
            {
                for (int i = 0; i < CardsArray.Count; i++)
                {
                    Card card = CardsArray[i];
                    card.IsDraggable = false;
                }
            }
        }

        public override void UpdateBackgroundColor()
        {
            if (CardsCount == 0)
            {
                return;
            }

            if (Type == DeckType.DECK_TYPE_BOTTOM)
            {
                for (int i = 0; i < CardsArray.Count; i++)
                {
                    Card card = CardsArray[i];
                    var colorBg = card.IsDraggable || !CardLogicComponent.HighlightDraggable
                        ? CardLogicComponent.DraggableColor
                        : CardLogicComponent.NondraggableColor;
                    card.SetBackgroundColor(colorBg);
                }
            }
            else if (Type == DeckType.DECK_TYPE_ACE || Type == DeckType.DECK_TYPE_FREECELL)
            {
                for (int i = 0; i < CardsArray.Count; i++)
                {
                    Card card = CardsArray[i];
                    card.SetBackgroundColor(CardLogicComponent.DraggableColor);
                }
            }
        }
    }
}
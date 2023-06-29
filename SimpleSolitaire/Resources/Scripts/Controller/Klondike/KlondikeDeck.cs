using SimpleSolitaire.Model.Enum;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    public class KlondikeDeck : Deck
    {
        /// <summary>
        /// Update card position in game by solitaire style
        /// </summary>
        /// <param name="firstTime">If it first game update</param>
        public override void UpdateCardsPosition(bool firstTime)
        {
            for (int i = 0; i < CardsArray.Count; i++)
            {
                Card card = (Card) CardsArray[i];
                card.transform.SetAsLastSibling();
                if (Type == DeckType.DECK_TYPE_PACK)
                {
                    card.IsDraggable = false;
                    card.gameObject.transform.position = gameObject.transform.position;
                    card.RestoreBackView();
                }
                else
                {
                    if (Type == DeckType.DECK_TYPE_ACE)
                    {
                        card.gameObject.transform.position = gameObject.transform.position;
                    }
                    else if (Type == DeckType.DECK_TYPE_WASTE)
                    {
                        var wasteHorizontalSpace = CardLogicComponent.GetSpaceFromDictionary(DeckSpacesTypes.DECK_SPACE_HORIONTAL_WASTE);
                        
                        card.IsDraggable = false;
                        card.gameObject.transform.position = gameObject.transform.position;
                        if (CardsArray.Count == 2)
                        {
                            if (i == 1)
                            {
                                card.gameObject.transform.position = gameObject.transform.position +
                                                                     new Vector3(wasteHorizontalSpace, 0, 0);
                                card.IsDraggable = true;
                            }
                        }
                        else if (CardsArray.Count >= 3)
                        {
                            if (i == CardsArray.Count - 1)
                            {
                                card.gameObject.transform.position = gameObject.transform.position +
                                                                     new Vector3(2 * wasteHorizontalSpace, 0, 0);
                                card.IsDraggable = true;
                            }
                            else if (i == CardsArray.Count - 2)
                            {
                                card.gameObject.transform.position = gameObject.transform.position +
                                                                     new Vector3(wasteHorizontalSpace, 0, 0);
                            }
                        }
                    }

                    if (i == CardsArray.Count - 1)
                    {
                        card.IsDraggable = true;
                        card.CardStatus = 1;
                        card.UpdateCardImg();
                    }
                    else
                    {
                        if (firstTime)
                        {
                            card.IsDraggable = false;
                            card.CardStatus = 0;
                        }

                        card.UpdateCardImg();
                    }
                }
            }

            if (Type == DeckType.DECK_TYPE_BOTTOM)
            {
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
                case DeckType.DECK_TYPE_BOTTOM:
                    if (topCard != null)
                    {
                        if (topCard.CardColor != card.CardColor && topCard.Number == card.Number + 1)
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
                        if (card.Number == 13)
                        {
                            return true;
                        }

                        return false;
                    }
                case DeckType.DECK_TYPE_ACE:
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
                bool isDraggable = true;
                topCard.IsDraggable = isDraggable;

                for (int i = CardsArray.Count - 2; i >= 0; i--)
                {
                    var card = CardsArray[i];
                    int nextNumber = card.Number;

                    if (card.CardStatus == 1 && nextNumber == topNumber + 1)
                    {
                        card.IsDraggable = isDraggable;
                        topNumber++;
                    }
                    else
                    {
                        isDraggable = false;
                        card.IsDraggable = isDraggable;
                    }
                }
            }
        }

        public override void UpdateBackgroundColor()
        {
            
        }
    }
}
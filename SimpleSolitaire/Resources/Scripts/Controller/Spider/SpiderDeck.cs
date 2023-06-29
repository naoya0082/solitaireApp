using SimpleSolitaire.Model.Enum;
using System.Collections.Generic;
using SimpleSolitaire.Model.Config;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    public class SpiderDeck : Deck
    {
        protected override void UpdateCardsActiveStatus()
        {
            if (Type == DeckType.DECK_TYPE_ACE || Type == DeckType.DECK_TYPE_WASTE)
            {
                int compareNum = 4;

                if (HasCards)
                {
                    int j = 0;
                    if (Type == DeckType.DECK_TYPE_PACK)
                    {
                        compareNum = 2;
                    }

                    for (int i = CardsArray.Count - 1; i >= 0; i--)
                    {
                        Card card = CardsArray[i];
                        if (j < compareNum)
                        {
                            card.gameObject.SetActive(true);
                            j++;
                        }
                        else
                        {
                            card.gameObject.SetActive(false);
                        }
                    }
                }
            }
            else if (Type == DeckType.DECK_TYPE_PACK)
            {
                //When add animation for dealing we must re-write this logic.
                int currentPackDealNumber = 0;
                int cardsInDeal = 10;
                for (int i = 0; i < CardsCount; i++)
                {
                    if ((i + 1) / cardsInDeal >= currentPackDealNumber)
                    {
                        currentPackDealNumber++;
                        CardsArray[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        CardsArray[i].gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                for (int i = CardsArray.Count - 1; i >= 0; i--)
                {
                    CardsArray[i].gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Update card position in game by solitaire style
        /// </summary>
        /// <param name="firstTime">If it first game update</param>
        public override void UpdateCardsPosition(bool firstTime)
        {
            if (CardsCount == 0)
            {
                return;
            }

            int dealAmount = 0;
            for (int i = 0; i < CardsArray.Count; i++)
            {
                Card card = CardsArray[i];
                card.transform.SetAsLastSibling();
                if (Type == DeckType.DECK_TYPE_PACK)
                {
                    var packHorizontalSpace =
                        CardLogicComponent.GetSpaceFromDictionary(DeckSpacesTypes.DECK_PACK_HORIZONTAL);
                    var dealNum = dealAmount + (i / 10);

                    card.gameObject.transform.position = gameObject.transform.position -
                                                         new Vector3(packHorizontalSpace * dealNum, 0, 0);
                    card.RestoreBackView();
                }
                else
                {
                    if (Type == DeckType.DECK_TYPE_ACE)
                    {
                        card.gameObject.transform.position = gameObject.transform.position;
                    }

                    if (i == CardsArray.Count - 1)
                    {
                        card.CardStatus = 1;
                    }
                    else
                    {
                        if (firstTime)
                        {
                            card.CardStatus = 0;
                        }
                    }

                    card.UpdateCardImg();
                }
            }

            if (Type == DeckType.DECK_TYPE_BOTTOM)
            {
                UpdateDraggableStatus();

                for (int i = 0; i < CardsArray.Count; i++)
                {
                    Card card = CardsArray[i];
                    Card prevCard = i > 0 ? CardsArray[i - 1] : null;

                    var space = prevCard != null && prevCard.CardStatus == 1
                        ? CardLogicComponent.GetSpaceFromDictionary(DeckSpacesTypes.DECK_SPACE_VERTICAL_BOTTOM_OPENED)
                        : CardLogicComponent.GetSpaceFromDictionary(DeckSpacesTypes.DECK_SPACE_VERTICAL_BOTTOM_CLOSED);
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
                        if (topCard.Number == card.Number + 1)
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
                        return true;
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

                    if (isDraggable && card.CardStatus == 1 && nextNumber == topNumber + 1 &&
                        card.CardType == topCard.CardType)
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

                UpdateBackgroundColor();
            }
            else if (Type == DeckType.DECK_TYPE_PACK)
            {
                for (int i = 0; i < CardsArray.Count; i++)
                {
                    Card card = CardsArray[i];
                    card.IsDraggable = false;
                }

                UpdateBackgroundColor();
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
                    var colorBg = card.IsDraggable || card.CardStatus == 0 || !CardLogicComponent.HighlightDraggable
                        ? CardLogicComponent.DraggableColor
                        : CardLogicComponent.NondraggableColor;
                    card.SetBackgroundColor(colorBg);
                    ;
                }
            }
            else if (Type == DeckType.DECK_TYPE_ACE || Type == DeckType.DECK_TYPE_PACK)
            {
                for (int i = 0; i < CardsArray.Count; i++)
                {
                    Card card = CardsArray[i];
                    card.SetBackgroundColor(CardLogicComponent.DraggableColor);
                }
            }
        }

        public List<Card> GetCompletedDeck()
        {
            if (Type == DeckType.DECK_TYPE_BOTTOM)
            {
                if (CardsCount == 0)
                {
                    return null;
                }

                List<Card> cards = new List<Card>();

                Card topCard = CardsArray[CardsArray.Count - 1];
                int topNumber = topCard.Number;
                cards.Add(topCard);

                for (int i = CardsArray.Count - 2; i >= 0; i--)
                {
                    var card = CardsArray[i];
                    int nextNumber = card.Number;

                    if (card.IsDraggable && nextNumber == topNumber + 1)
                    {
                        cards.Add(card);
                        topNumber++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (cards.Count == Public.CARD_NUMS_OF_SUIT)
                {
                    return cards;
                }
            }

            return null;
        }
    }
}
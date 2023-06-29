using SimpleSolitaire.Model.Enum;
using System.Collections.Generic;
using SimpleSolitaire.Model.Config;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimpleSolitaire.Controller
{
    public abstract class Deck : MonoBehaviour, IPointerClickHandler
    {
        public CardLogic CardLogicComponent;
        public int DeckNum = 0;
        public DeckType Type = 0;
        public List<Card> CardsArray = new List<Card>();

        public bool HasCards => CardsArray.Count > 0;
        public int CardsCount => CardsArray.Count;

        [Space(5f)] [SerializeField] private Image _backgroundImage;
        [SerializeField] private GameManager _gameManagerComponent;

        /// <summary>
        /// Set up background image for deck <see cref="_backgroundImage"/>
        /// </summary>
        /// <param name="str">string name of deck</param>
        public void SetBackgroundImg(string str)
        {
            Sprite tempType = CardLogicComponent.LoadSprite(Public.PATH_TO_DECKS_IN_RESOURCES + str);
            _backgroundImage.sprite = tempType;
        }

        /// <summary>
        /// Show/Add in game  new card from pack.
        /// </summary>
        /// <param name="card"></param>
        public void PushCard(Card card)
        {
            card.Deck = this;
            card.IsDraggable = true;
            card.CardStatus = 1;
            CardsArray.Add(card);
        }

        /// <summary>
        /// Show/Add in game new card array from pack.
        /// </summary>
        /// <param name="card"></param>
        public void PushCardArray(Card[] cardArray, bool isDraggable = true, int cardStatus = 1)
        {
            for (int i = 0; i < cardArray.Length; i++)
            {
                cardArray[i].Deck = this;
                cardArray[i].IsDraggable = isDraggable;
                cardArray[i].CardStatus = cardStatus;
                CardsArray.Add(cardArray[i]);
            }
        }

        public void PushCardArray(List<Card> cardArray, bool isDraggable = true, int cardStatus = 1)
        {
            for (int i = 0; i < cardArray.Count; i++)
            {
                cardArray[i].Deck = this;
                cardArray[i].IsDraggable = isDraggable;
                cardArray[i].CardStatus = cardStatus;
                CardsArray.Add(cardArray[i]);
            }
        }

        /// <summary>
        /// Return last card from pack.
        /// </summary>
        public Card Pop()
        {
            Card retCard = null;
            int count = CardsArray.Count;
            if (count > 0)
            {
                retCard = CardsArray[count - 1];
                retCard.Deck = null;
                CardsArray.Remove(retCard);
            }

            return retCard;
        }

        public void Clear()
        {
            int count = CardsArray.Count;
            if (count == 0)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                Card toRemove = CardsArray[i];
                toRemove.Deck = null;
            }

            CardsArray.Clear();
        }

        public void RemoveFromArray(List<Card> cardsArray)
        {
            int count = CardsArray.Count;
            if (count == 0)
            {
                return;
            }

            for (int i = 0; i < cardsArray.Count; i++)
            {
                Card toRemove = cardsArray[i];
                if (CardsArray.Contains(toRemove))
                {
                    toRemove.Deck = null;
                    CardsArray.Remove(toRemove);
                }
            }
        }

        /// <summary>
        /// Get card array from pop.
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public Card[] PopFromCard(Card card)
        {
            int i = 0;
            int count = CardsArray.Count;
            while (i < count)
            {
                if ((Card) CardsArray[i] == card)
                {
                    break;
                }

                i++;
            }

            Card[] cardArray = new Card[count - i];
            int k = 0;
            for (int j = i; j < count; j++)
            {
                cardArray[count - i - 1 - (k++)] = Pop();
            }

            return cardArray;
        }
        
        public int CardsAmountFromCard(Card card)
        {
            int i = 0;
            int count = CardsArray.Count;
            while (i < count)
            {
                if (CardsArray[i] == card)
                {
                    break;
                }

                i++;
            }

            return count - i;
        }

        /// <summary>
        /// Update card position in game by solitaire style
        /// </summary>
        /// <param name="firstTime">If it first game update</param>
        public abstract void UpdateCardsPosition(bool firstTime);

        /// <summary>
        /// After set positions <see cref="UpdateCardsPosition(bool)"/> game show for user available cards and not available.
        /// </summary>
        protected virtual void UpdateCardsActiveStatus()
        {
            int compareNum = 4;
            if (Type == DeckType.DECK_TYPE_ACE || Type == DeckType.DECK_TYPE_WASTE || Type == DeckType.DECK_TYPE_PACK)
            {
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
            else
            {
                for (int i = CardsArray.Count - 1; i >= 0; i--)
                {
                    (CardsArray[i]).gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Set new position for card holder.
        /// </summary>
        /// <param name="card">Card for change position</param>
        /// <param name="x">Position by X axis</param>
        /// <param name="y">Position by Y axis</param>
        public void SetPositionFromCard(Card card, float x, float y)
        {
            int i;
            for (i = 0; i < CardsArray.Count; i++)
            {
                if (CardsArray[i] == card)
                {
                    break;
                }
            }

            var verticalSpace =
                CardLogicComponent.GetSpaceFromDictionary(DeckSpacesTypes.DECK_SPACE_VERTICAL_BOTTOM_OPENED);
            int m = 0;
            for (int j = i; j < CardsArray.Count; j++)
            {
                (CardsArray[j]).SetPosition(new Vector3(x, y - m++ * verticalSpace, 0));
            }
        }

        /// <summary>
        /// Collect card on aceDeck.
        /// </summary>
        /// <param name="card">Card for collect.</param>
        public void SetCardsToTop(Card card)
        {
            bool found = false;
            for (int i = 0; i < CardsArray.Count; i++)
            {
                if (CardsArray[i] == card)
                {
                    found = true;
                }

                if (found)
                {
                    ((Card) CardsArray[i]).transform.SetAsLastSibling();
                }
            }
        }

        /// <summary>
        /// Get last card on aceDeck.
        /// </summary>
        /// <returns></returns>
        public Card GetTopCard()
        {
            if (HasCards)
            {
                return CardsArray[CardsCount - 1];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get previous last card on deck.
        /// </summary>
        /// <returns></returns>
        public Card GetPreviousFromCard(Card fromCard)
        {
            int index = CardsArray.IndexOf(fromCard);

            if (index >= 1)
            {
                return CardsArray[index - 1];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If we can drop card to other card it will be true.
        /// </summary>
        /// <param name="card">Checking card</param>
        /// <returns>We can drop or no</returns>
        public abstract bool AcceptCard(Card card);

        /// <summary>
        /// If we drop our card on other card this method return true.
        /// </summary>
        /// <param name="card">Checking card</param>
        public bool OverlapWithCard(Card card)
        {
            if (card.Deck == this)
            {
                return false;
            }

            bool bOverlaped = false;
            float x1 = transform.position.x;
            float x2 = x1 + CardLogicComponent.DeckWidth;
            float y1 = transform.position.y;
            Card topCard = GetTopCard();
            if (topCard)
            {
                y1 = topCard.transform.position.y;
            }

            float y2 = y1 + CardLogicComponent.DeckHeight;

            float x11 = card.transform.position.x;
            float x21 = x11 + CardLogicComponent.DeckWidth;
            float y11 = card.transform.position.y;
            float y21 = y11 + CardLogicComponent.DeckHeight;

            float INTERSECT_SPACE = 10;
            if ((x2 >= (x11 + INTERSECT_SPACE) && x1 <= x11) || (x1 >= x11 && x1 <= (x21 - INTERSECT_SPACE)))
            {
                if ((y1 >= y11 && y1 <= y21) || (y1 <= y11 && y2 >= y11))
                {
                    bOverlaped = true;
                }
            }

            return bOverlaped;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Type == DeckType.DECK_TYPE_PACK)
            {
                CardLogicComponent.OnClickPack();
            }
        }

        /// <summary>
        /// Initialize first game state.
        /// </summary>
        public void RestoreInitialState()
        {
            for (int i = 0; i < CardsArray.Count; i++)
            {
                Card card = CardsArray[i];
                card.RestoreBackView();
            }

            CardsArray.Clear();
        }

        public abstract void UpdateDraggableStatus();
        public abstract void UpdateBackgroundColor();
        
    }
}
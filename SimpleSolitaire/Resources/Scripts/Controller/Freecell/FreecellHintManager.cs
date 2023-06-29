using SimpleSolitaire.Model;
using SimpleSolitaire.Model.Enum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    public class FreecellHintManager : HintManager
    {
        private FreecellCardLogic FreecellLogic => _cardLogicComponent as FreecellCardLogic;

        protected override IEnumerator HintTranslate(HintData data)
        {
            IsHintProcess = true;

            List<HintElement> hints = data.Type == HintType.AutoComplete ? AutoCompleteHints : Hints;
            if (data.Type == HintType.AutoComplete) CurrentHintIndex = 0;
            if (data.Card != null) CurrentHintIndex = hints.FindIndex(x => x.HintCard == data.Card);

            if (data.Card != null && CurrentHintIndex == -1)
            {
                AudioController audioCtrl = AudioController.Instance;
                if (audioCtrl != null)
                {
                    audioCtrl.Play(AudioController.AudioType.Error);
                }

                Debug.LogWarning("After double tap! This Card: " + data.Card.CardNumber +
                                 " is not available for complete to ace pack.");
                IsHintProcess = false;
                CurrentHintIndex = 0;
                yield break;
            }

            var t = 0f;
            Card hintCard = hints[CurrentHintIndex].HintCard;
            hintCard.Deck.UpdateCardsPosition(false);

            CurrentHintSiblingIndex = hintCard.transform.GetSiblingIndex();

            hintCard.Deck.SetCardsToTop(hintCard);

            while (t < 1)
            {
                t += Time.deltaTime / data.HintTime;
                hintCard.transform.position = Vector3.Lerp(hints[CurrentHintIndex].FromPosition,
                    hints[CurrentHintIndex].ToPosition, t);

                yield return new WaitForEndOfFrame();
                hints[CurrentHintIndex].HintCard.Deck.SetPositionFromCard(hintCard,
                    hintCard.transform.position.x,
                    hintCard.transform.position.y);
            }

            if (IsHasHint() && data.Type == HintType.Hint)
            {
                hintCard.Deck.UpdateCardsPosition(false);
                hintCard.transform.position = hints[CurrentHintIndex].FromPosition;
                hintCard.transform.SetSiblingIndex(CurrentHintSiblingIndex);
                CurrentHintIndex = CurrentHintIndex == hints.Count - 1 ? CurrentHintIndex = 0 : CurrentHintIndex + 1;
            }

            if (data.Type != HintType.Hint)
            {
                _cardLogicComponent.OnDragEnd(hintCard);
            }

            IsHintProcess = false;
        }

        /// <summary>
        /// Generate new hint depending on available for move cards.
        /// </summary>
        protected override void GenerateHints(bool isAutoComplete = false)
        {
            CurrentHintIndex = 0;
            AutoCompleteHints = new List<HintElement>();
            Hints = new List<HintElement>();
            bool isHasAutoCompleteHints;

            if (IsAvailableForMoveCardArray.Count > 0)
            {
                int borderForMove = FreecellLogic != null ? FreecellLogic.GetSuperMovesAmount() : 0;

                foreach (var card in IsAvailableForMoveCardArray)
                {
                    for (int i = 0; i < _cardLogicComponent.AllDeckArray.Length; i++)
                    {
                        isHasAutoCompleteHints = true;
                        Deck targetDeck = _cardLogicComponent.AllDeckArray[i];
                        Deck srcDeck = card.Deck;
                        if (targetDeck.Type == DeckType.DECK_TYPE_BOTTOM ||
                            targetDeck.Type == DeckType.DECK_TYPE_ACE ||
                            targetDeck.Type == DeckType.DECK_TYPE_FREECELL)
                        {
                            if (targetDeck.Type == DeckType.DECK_TYPE_BOTTOM)
                            {
                                int cardsForAccept = card.Deck.CardsAmountFromCard(card);

                                if (cardsForAccept > borderForMove)
                                {
                                    continue;
                                }
                            }

                            if (targetDeck.Type == DeckType.DECK_TYPE_FREECELL)
                            {
                                Card topCard = srcDeck.GetTopCard();
                                if (topCard != card)
                                {
                                    continue;
                                }
                            }

                            if (card != null)
                            {
                                Card topTargetDeckCard = targetDeck.GetTopCard();
                                Card prevCard = card.Deck.GetPreviousFromCard(card);

                                if (card.Deck.Type == DeckType.DECK_TYPE_ACE)
                                {
                                    continue;
                                }

                                if (topTargetDeckCard == null && (targetDeck.Type == DeckType.DECK_TYPE_BOTTOM ||
                                                                  targetDeck.Type == DeckType.DECK_TYPE_FREECELL))
                                {
                                    isHasAutoCompleteHints = false;

                                    if (isAutoComplete)
                                    {
                                        continue;
                                    }
                                }

                                if (prevCard == null && topTargetDeckCard == null &&
                                    targetDeck.Type != DeckType.DECK_TYPE_ACE)
                                {
                                    isHasAutoCompleteHints = false;

                                    if (isAutoComplete)
                                    {
                                        continue;
                                    }
                                }

                                if (prevCard != null && topTargetDeckCard != null &&
                                    prevCard.Number == topTargetDeckCard.Number && prevCard.CardStatus == 1 && 
                                    topTargetDeckCard.CardColor == prevCard.CardColor)
                                {
                                    isHasAutoCompleteHints = false;

                                    if (isAutoComplete)
                                    {
                                        continue;
                                    }
                                }

                                if (targetDeck.AcceptCard(card))
                                {
                                    var offset = GetHintSpace(topTargetDeckCard);
                                    if (isHasAutoCompleteHints)
                                    {
                                        AutoCompleteHints.Add(new HintElement(card, card.transform.position,
                                            topTargetDeckCard != null
                                                ? topTargetDeckCard.transform.position - offset
                                                : targetDeck.transform.position, targetDeck));
                                    }

                                    Hints.Add(new HintElement(card, card.transform.position,
                                        topTargetDeckCard != null
                                            ? topTargetDeckCard.transform.position - offset
                                            : targetDeck.transform.position, targetDeck));
                                }
                            }
                        }
                    }
                }
            }

            ActivateHintButton(IsHasHint());
            ActivateAutoCompleteHintButton(IsHasAutoCompleteHint());
        }
    }
}
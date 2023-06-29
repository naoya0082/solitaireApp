using SimpleSolitaire.Model;
using SimpleSolitaire.Model.Enum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    public class SpiderHintManager : HintManager
    {
        private KeyValuePair<Card, List<Deck>> _lastHintCard = new KeyValuePair<Card, List<Deck>>();

        protected override IEnumerator HintTranslate(HintData data)
        {
            IsHintProcess = true;
            
            bool isNeedResetLastHintData = false;
            List<HintElement> hints = data.Type == HintType.AutoComplete ? AutoCompleteHints : Hints;
            if (data.Type == HintType.AutoComplete) CurrentHintIndex = 0;
            else if (data.Type == HintType.CardClick) CurrentHintIndex = -1;
            if (data.Card != null && data.Type == HintType.CardClick)
            {
                if (_lastHintCard.Key != null && _lastHintCard.Key != data.Card)
                {
                    _lastHintCard = new KeyValuePair<Card, List<Deck>>();
                }

                for (int i = 0; i < hints.Count; i++)
                {
                    var hint = hints[i];
                    if (hint.HintCard == data.Card && (_lastHintCard.Value == null ||
                                                       !_lastHintCard.Value.Contains(hint.DestinationDeck)))
                    {
                        CurrentHintIndex = i;
                        break;
                    }
                }

                if (CurrentHintIndex == -1)
                {
                    isNeedResetLastHintData = true;
                    for (int i = 0; i < hints.Count; i++)
                    {
                        var hint = hints[i];
                        if (hint.HintCard == data.Card)
                        {
                            CurrentHintIndex = i;
                            break;
                        }
                    }
                }
            }

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
            HintElement hintElement = hints[CurrentHintIndex];
            Card hintCard = hintElement.HintCard;

            if (data.Type == HintType.CardClick)
            {
                if (isNeedResetLastHintData)
                {
                    _lastHintCard = new KeyValuePair<Card, List<Deck>>();
                }
                else
                {
                    if (_lastHintCard.Key == null)
                    {
                        _lastHintCard =
                            new KeyValuePair<Card, List<Deck>>(data.Card, new List<Deck>() {hintElement.HintCard.Deck});
                    }
                    else
                    {
                        if (_lastHintCard.Value != null && !_lastHintCard.Value.Contains(hintElement.HintCard.Deck))
                        {
                            _lastHintCard.Value.Add(hintElement.HintCard.Deck);
                        }
                    }
                }
            }

            hintCard.Deck.UpdateCardsPosition(false);

            CurrentHintSiblingIndex = hintCard.transform.GetSiblingIndex();

            hintCard.Deck.SetCardsToTop(hintCard);

            while (t < 1)
            {
                t += Time.deltaTime / data.HintTime;
                hintCard.transform.position = Vector3.Lerp(hintElement.FromPosition,
                    hintElement.ToPosition, t);

                yield return new WaitForEndOfFrame();
                hintElement.HintCard.Deck.SetPositionFromCard(hintCard,
                    hintCard.transform.position.x,
                    hintCard.transform.position.y);
            }

            if (IsHasHint() && data.Type == HintType.Hint)
            {
                hintCard.Deck.UpdateCardsPosition(false);
                hintCard.transform.position = hintElement.FromPosition;
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
                foreach (var card in IsAvailableForMoveCardArray)
                {
                    for (int i = 0; i < _cardLogicComponent.AllDeckArray.Length; i++)
                    {
                        isHasAutoCompleteHints = true;
                        Deck targetDeck = _cardLogicComponent.AllDeckArray[i];
                        if (targetDeck.Type == DeckType.DECK_TYPE_BOTTOM
                            || targetDeck.Type == DeckType.DECK_TYPE_ACE)
                        {
                            if (card != null)
                            {
                                Card topTargetDeckCard = targetDeck.GetTopCard();
                                Card topDeckCard = card.Deck.GetPreviousFromCard(card);

                                if (card.Deck.Type == DeckType.DECK_TYPE_ACE)
                                {
                                    continue;
                                }

                                if (topDeckCard == null && topTargetDeckCard == null &&
                                    targetDeck.Type != DeckType.DECK_TYPE_ACE)
                                {
                                    if (card.Deck.Type != DeckType.DECK_TYPE_WASTE)
                                    {
                                        isHasAutoCompleteHints = false;

                                        if (isAutoComplete)
                                        {
                                            continue;
                                        }
                                    }
                                }

                                if (topDeckCard != null && topTargetDeckCard != null &&
                                    topDeckCard.Number == topTargetDeckCard.Number && topDeckCard.CardStatus == 1 &&
                                    card.Deck.Type != DeckType.DECK_TYPE_WASTE || 
                                    targetDeck.Type == DeckType.DECK_TYPE_BOTTOM && !targetDeck.HasCards)
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
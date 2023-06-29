using SimpleSolitaire.Model;
using SimpleSolitaire.Model.Enum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace SimpleSolitaire.Controller
{
    public enum HintType
    {
        Hint,
        CardClick,
        AutoComplete
    }

    public class HintData
    {
        public float HintTime;
        public HintType Type;
        public Card Card;
        public bool HintButtonPressed;

        public HintData(float hintTime, HintType type, Card card, bool hintButtonPressed)
        {
            HintTime = hintTime;
            Type = type;
            Card = card;
            HintButtonPressed = hintButtonPressed;
        }
    }

    public abstract class HintManager : MonoBehaviour
    {
        [Header("Components: ")] [SerializeField]
        protected CardLogic _cardLogicComponent;

        [SerializeField] private AutoCompleteManager _autoCompleteComponent;
        //[SerializeField] private InterVideoAds _adsManager;
        [SerializeField] private Button _hintButton;

        [Header("Hint data: ")] public List<Card> IsAvailableForMoveCardArray = new List<Card>();
        public List<HintElement> Hints = new List<HintElement>();
        public List<HintElement> AutoCompleteHints = new List<HintElement>();
        public int CurrentHintIndex = 0;
        public int CurrentHintSiblingIndex;
        public bool IsHintProcess = false;
        public bool IsHintWasUsed = false;
        [Header("Settings: ")] public float TapToPlaceTranslateTime = 0.2f;
        public float HintTranslateTime = 0.75f;

        private IEnumerator HintCoroutine;

        /// <summary>
        /// Call hint animation.
        /// </summary>
        protected void Hint(HintData data)
        {
            if (Hints.Count > 0 && !IsHintProcess && gameObject.activeInHierarchy)
            {
                if (data.HintButtonPressed)
                {
                    //_adsManager.TryShowIntersitialByCounter();
                    
                    AudioController audioCtrl = AudioController.Instance;

                    if (audioCtrl != null)
                    {
                        audioCtrl.Play(AudioController.AudioType.Hint);
                    }
                }

                if (HintCoroutine != null)
                {
                    IsHintProcess = false;
                    StopCoroutine(HintCoroutine);
                }

                HintCoroutine = HintTranslate(data);
                StartCoroutine(HintCoroutine);
            }
        }

        /// <summary>
        /// Called when user click on any card.
        /// </summary>
        public void HintAndSetByClick(Card card)
        {
            var data = new HintData(
                hintTime: TapToPlaceTranslateTime,
                type: HintType.CardClick,
                card: card,
                hintButtonPressed: false
            );
            Hint(data);
        }

        /// <summary>
        /// Called automatically when auto complete action is active.
        /// </summary>
        public void HintAndSet(float time = 0.75f)
        {
            var data = new HintData(
                hintTime: time,
                type: HintType.AutoComplete,
                card: null,
                hintButtonPressed: false
            );
            Hint(data);
        }

        /// <summary>
        /// Called when user press hint button.
        /// </summary>
        public void HintButtonAction()
        {
            var data = new HintData(
                hintTime: HintTranslateTime,
                type: HintType.Hint,
                card: null,
                hintButtonPressed: true
            );
            Hint(data);
        }

        protected abstract IEnumerator HintTranslate(HintData data);

        /// <summary>
        /// Update for user drag hints.
        /// </summary>
        /// <param name="isAutoComplete"></param>
        public virtual void UpdateAvailableForDragCards(bool isAutoComplete = false)
        {
            IsAvailableForMoveCardArray = new List<Card>();

            Card[] cards = _cardLogicComponent.CardsArray;

            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i].IsDraggable)
                {
                    if (cards[i].Deck.Type != DeckType.DECK_TYPE_ACE)
                    {
                        IsAvailableForMoveCardArray.Add(cards[i]);
                    }
                    else if (!isAutoComplete)
                    {
                        if (cards[i].Deck.GetTopCard() == cards[i])
                        {
                            IsAvailableForMoveCardArray.Add(cards[i]);
                        }
                    }
                }
            }

            GenerateHints();
        }

        /// <summary>
        /// Update auto complete hints
        /// </summary>
        public virtual void UpdateAvailableForAutoCompleteCards()
        {
            IsAvailableForMoveCardArray = new List<Card>();

            Card[] cards = _cardLogicComponent.CardsArray;

            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i].IsDraggable)
                {
                    if (cards[i].Deck.Type != DeckType.DECK_TYPE_ACE)
                    {
                        IsAvailableForMoveCardArray.Add(cards[i]);
                    }
                }
            }

            GenerateHints(true);
        }

        /// <summary>
        /// Generate new hint depending on available for move cards.
        /// </summary>
        protected abstract void GenerateHints(bool isAutoComplete = false);

        public Card GetCurrentHintCard(bool isAutoComplete)
        {
            List<HintElement> hints = isAutoComplete ? AutoCompleteHints : Hints;

            return hints.Count > 0 ? hints[0].HintCard : null;
        }

        /// <summary>
        /// Reset all hints.
        /// </summary>
        public void ResetHint()
        {
            if (HintCoroutine != null)
            {
                StopCoroutine(HintCoroutine);
            }

            IsHintProcess = false;

            if (IsHintWasUsed)
            {
                Hints[CurrentHintIndex].HintCard.Deck.UpdateCardsPosition(false);

                Hints[CurrentHintIndex].HintCard.transform.localPosition = Hints[CurrentHintIndex].FromPosition;
                Hints[CurrentHintIndex].HintCard.transform.SetSiblingIndex(CurrentHintSiblingIndex);
            }
        }

        /// <summary>
        /// Activate for user hint button on bottom panel.
        /// </summary>
        protected void ActivateHintButton(bool isActive)
        {
            _hintButton.interactable = isActive;
        }

        /// <summary>
        /// Activate auto complete button if auto complete hints is available. 
        /// </summary>
        protected void ActivateAutoCompleteHintButton(bool isActive)
        {
            if (isActive)
            {
                _autoCompleteComponent.ActivateAutoCompleteAvailability();
            }
            else
            {
                _autoCompleteComponent.DeactivateAutoCompleteAvailability();
            }
        }

        /// <summary>
        /// Is has available hints for user.
        /// </summary>
        public bool IsHasHint()
        {
            return Hints.Count > 0;
        }

        /// <summary>
        /// Check for availability of auto complete hints.
        /// </summary>
        /// <returns></returns>
        public bool IsHasAutoCompleteHint()
        {
            return AutoCompleteHints.Count > 0;
        }
        
        protected virtual Vector3 GetHintSpace(Card targetCard)
        {
            if (targetCard == null)
            {
                return  Vector3.zero;
            }

            switch (targetCard.Deck.Type)
            {
                case DeckType.DECK_TYPE_BOTTOM:
                {
                    var spaceY = _cardLogicComponent.GetSpaceFromDictionary(DeckSpacesTypes.DECK_SPACE_VERTICAL_BOTTOM_OPENED);
                    return new Vector3(0,spaceY,0);

                }
                default:
                    return Vector3.zero;
            }
        }

        protected void OnDestroy()
        {
            if (HintCoroutine != null)
            {
                StopCoroutine(HintCoroutine);
            }
        }
    }
}
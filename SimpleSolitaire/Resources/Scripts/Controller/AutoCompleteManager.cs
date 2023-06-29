using System.Collections;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    public enum AutoCompleteMode
    {
        FullGameSession = 0,
        OnlyWhenAllDecksClear
    }

    public class AutoCompleteManager : MonoBehaviour
    {
        [Tooltip("The mode of auto complete actions. Activates for full game session and only when all decks clear.")]
        public AutoCompleteMode Mode;
        
        [Tooltip("The state of auto complete actions.")]
        public bool IsAutoCompleteActive = false;

        [Tooltip("Time between cards sets on correct place. (Transition)")]
        public float HintSetTransitionTime = 0.2f;

        [Header("Components")]
        public HintManager HintComponent;
        public CardLogic CardLogicComponent;
        public GameObject AutoCompleteHintButtonObj;

        private IEnumerator _doubleClickAutoCompleteCoroutine;
        private IEnumerator _autoCompleteCoroutine;
        private bool _isCanComplete = true;

        private bool _autoCompleteFeatureEnable = true;

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                CompleteGame();
            }
        }

        public void SetEnableAutoCompleteFeature(bool state)
        {
            _autoCompleteFeatureEnable = state;
            AutoCompleteHintButtonObj.SetActive(_isCanComplete && state && CheckAvailabilityByMode());
        }

        /// <summary>
        /// Activate autocomplete availability with button.
        /// </summary>
        public void ActivateAutoCompleteAvailability()
        {
            _isCanComplete = true;

            if (!_autoCompleteFeatureEnable)
            {
                return;
            }

            bool isAvailable = CheckAvailabilityByMode();

            AutoCompleteHintButtonObj.SetActive(isAvailable);
        }

        private bool CheckAvailabilityByMode()
        {
            if (Mode == AutoCompleteMode.OnlyWhenAllDecksClear)
            {
                bool isAllDecksClear = true;

                for (int i = 0; i < CardLogicComponent.BottomDeckArray.Length; i++)
                {
                    var deck = CardLogicComponent.BottomDeckArray[i];
                    for (int j = 0; j < deck.CardsArray.Count; j++)
                    {
                        var card = deck.CardsArray[j];
                        if (!card.IsDraggable)
                        {
                            isAllDecksClear = false;
                        }
                    }
                }
                
                if (!isAllDecksClear)
                {
                    return false;
                }

                isAllDecksClear = (CardLogicComponent.PackDeck != null && CardLogicComponent.PackDeck.CardsCount == 0) 
                                  &&  (CardLogicComponent.WasteDeck == null || CardLogicComponent.WasteDeck.CardsArray.Count <= 1);
                
                if (!isAllDecksClear)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Deactivate autocomplete availability with button.
        /// </summary>
        public void DeactivateAutoCompleteAvailability()
        {
            _isCanComplete = false;

            if (!_autoCompleteFeatureEnable)
            {
                return;
            }
            AutoCompleteHintButtonObj.SetActive(false);
        }

        /// <summary>
        /// Call auto complete action.
        /// </summary>
        public void CompleteGame()
        {
            if (_isCanComplete)
            {
                _isCanComplete = false;
                StopAutoComplete();
                _autoCompleteCoroutine = CompleteCoroutine();
                StartCoroutine(_autoCompleteCoroutine);
            }
        }

        /// <summary>
        /// Auto complete actions in coroutine.
        /// </summary>
        private IEnumerator CompleteCoroutine()
        {
            IsAutoCompleteActive = true;
            HintComponent.UpdateAvailableForAutoCompleteCards();

            while (HintComponent.IsHasHint())
            {
                HintComponent.HintAndSet(HintSetTransitionTime);

                yield return new WaitWhile(() => HintComponent.IsHintProcess);
            }

            IsAutoCompleteActive = false;
            HintComponent.UpdateAvailableForDragCards();
        }

        /// <summary>
        /// Deactivate auto complete coroutine.
        /// </summary>
        private void StopAutoComplete()
        {
            if (_autoCompleteCoroutine != null)
            {
                StopCoroutine(_autoCompleteCoroutine);
            }
        }
    }
}
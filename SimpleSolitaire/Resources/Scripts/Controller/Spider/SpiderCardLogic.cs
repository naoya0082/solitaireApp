using SimpleSolitaire.Model.Config;
using SimpleSolitaire.Model.Enum;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleSolitaire.Controller
{
    public enum SpiderSuitsType
    {
        OneSuit = 0, //Only spade
        TwoSuits = 1, //Spade and heart
        FourSuits //Spade, heart, club and diamonds
    }

    public class SpiderCardLogic : CardLogic
    {
        protected override int CardNums => Public.SPIDER_CARD_NUMS;

        [Space, Header("Spider logic fields: ")]
        public SpiderSuitsType CurrentSpiderSuitsType;

        private SpiderSuitsType TempSuitsType { get; set; }

        [Header("Spider suit toggles:")] [SerializeField]
        private Toggle _oneSuitToggle;

        [SerializeField] private Toggle _twoSuitsToggle;
        [SerializeField] private Toggle _fourSuitsToggle;

        private void ChangeSuitsTypeByToggle(SpiderSuitsType type)
        {
            if (CurrentSpiderSuitsType == type) return;

            TempSuitsType = type;
        }

        public void InitSuitsToggles()
        {
            TempSuitsType = CurrentSpiderSuitsType;

            _oneSuitToggle.SetIsOnWithoutNotify(CurrentSpiderSuitsType == SpiderSuitsType.OneSuit);
            _twoSuitsToggle.SetIsOnWithoutNotify(CurrentSpiderSuitsType == SpiderSuitsType.TwoSuits);
            _fourSuitsToggle.SetIsOnWithoutNotify(CurrentSpiderSuitsType == SpiderSuitsType.FourSuits);
        }

        public override void InitCardLogic()
        {
            InitSuitsToggles();

            base.InitCardLogic();
        }

        public override void InitializeSpacesDictionary()
        {
            base.InitializeSpacesDictionary();
            
            SpacesDict.Add(DeckSpacesTypes.DECK_SPACE_VERTICAL_BOTTOM_OPENED, DeckHeight / 3.5f);
            SpacesDict.Add(DeckSpacesTypes.DECK_SPACE_VERTICAL_BOTTOM_CLOSED, DeckHeight / 3.5f / 2);
            SpacesDict.Add(DeckSpacesTypes.DECK_PACK_HORIZONTAL, DeckWidth / 3.0f);
        }

        public override void SubscribeEvents()
        {
            _oneSuitToggle.onValueChanged.AddListener(delegate { ChangeSuitsTypeByToggle(SpiderSuitsType.OneSuit); });
            _twoSuitsToggle.onValueChanged.AddListener(delegate { ChangeSuitsTypeByToggle(SpiderSuitsType.TwoSuits); });
            _fourSuitsToggle.onValueChanged.AddListener(
                delegate { ChangeSuitsTypeByToggle(SpiderSuitsType.FourSuits); });
        }

        public override void UnsubscribeEvents()
        {
            _oneSuitToggle.onValueChanged.RemoveAllListeners();
            _twoSuitsToggle.onValueChanged.RemoveAllListeners();
            _fourSuitsToggle.onValueChanged.RemoveAllListeners();
        }

        public override void OnNewGameStart()
        {
            CurrentSpiderSuitsType = TempSuitsType;
            IsGameStarted = true;
        }

        public void SetSuitsImmediately(SpiderSuitsType type)
        {
            TempSuitsType = type;
            CurrentSpiderSuitsType = TempSuitsType;
        }

        /// <summary>
        /// Initialize deck of cards.
        /// </summary>
        protected override void InitDeckCards()
        {
            for (int i = 0; i < BottomDeckArray.Length; i++)
            {
                int cardCount = i < 4 ? 6 : 5;
                Deck bottomDeck = BottomDeckArray[i];

                for (int j = 0; j < cardCount; j++)
                {
                    bottomDeck.PushCard(PackDeck.Pop());
                }

                bottomDeck.UpdateDraggableStatus();
                bottomDeck.UpdateCardsPosition(true);
            }

            if (PackDeck != null)
            {
                PackDeck.UpdateCardsPosition(true);
                PackDeck.UpdateDraggableStatus();
            }
        }

        /// <summary>
        /// Call when we drop card.
        /// </summary>
        /// <param name="card">Dropped card</param>
        public override void OnDragEnd(Card card)
        {
            bool isPackWasteNotFound = false;
            bool isHasTarget = false;

            for (int i = 0; i < AllDeckArray.Length; i++)
            {
                SpiderDeck targetDeck = AllDeckArray[i] as SpiderDeck;
                if (targetDeck == null)
                {
                    continue;
                }

                if (targetDeck.Type == DeckType.DECK_TYPE_BOTTOM || targetDeck.Type == DeckType.DECK_TYPE_ACE)
                {
                    if (targetDeck.OverlapWithCard(card))
                    {
                        isHasTarget = true;
                        Deck srcDeck = card.Deck;

                        if (targetDeck.AcceptCard(card))
                        {
                            WriteUndoState();
                            Card[] popCards = srcDeck.PopFromCard(card);
                            targetDeck.PushCardArray(popCards);
                            targetDeck.UpdateCardsPosition(false);
                            srcDeck.UpdateCardsPosition(false);

                            srcDeck.UpdateDraggableStatus();

                            TryCompleteDeck(targetDeck);

                            ActionAfterEachStep();

                            GameManagerComponent.AddScoreValue(Public.SCORE_MOVE_TO);
                            AudioCtrl.Play(AudioController.AudioType.Move);

                            return;
                        }
                    }
                }
                else
                {
                    isPackWasteNotFound = true;
                }
            }

            if (isPackWasteNotFound &&
                (card.Deck.Type != DeckType.DECK_TYPE_PACK) ||
                isHasTarget)
            {
                if (AudioCtrl != null)
                {
                    AudioCtrl.Play(AudioController.AudioType.Error);
                }
            }
        }

        /// <summary>
        /// Call when we click on pack with cards.
        /// </summary>
        public override void OnClickPack()
        {
            bool cantPush = BottomDeckArray.Any(x => x.CardsCount == 0) || !PackDeck.HasCards;

            if (cantPush)
            {
                if (AudioCtrl != null)
                {
                    AudioCtrl.Play(AudioController.AudioType.Error);
                }

                return;
            }

            WriteUndoState();

            for (int i = 0; i < BottomDeckArray.Length; i++)
            {
                SpiderDeck bottomDeck = BottomDeckArray[i] as SpiderDeck;
                if (bottomDeck == null)
                {
                    continue;
                }

                bottomDeck.PushCard(PackDeck.Pop());
                bottomDeck.UpdateCardsPosition(false);
                bottomDeck.UpdateDraggableStatus();
                TryCompleteDeck(bottomDeck);

                if (AudioCtrl != null)
                {
                    AudioCtrl.Play(AudioController.AudioType.Move);
                }
            }

            PackDeck.UpdateCardsPosition(false);

            ActionAfterEachStep();

            if (AudioCtrl != null)
            {
                AudioCtrl.Play(AudioController.AudioType.Move);
            }
        }

        private void TryCompleteDeck(SpiderDeck srcDeck)
        {
            var completedDeck = srcDeck.GetCompletedDeck();

            if (completedDeck == null)
            {
                return;
            }

            for (int j = 0; j < AceDeckArray.Length; j++)
            {
                var aceDeck = AceDeckArray[j];
                if (aceDeck.HasCards)
                {
                    continue;
                }

                srcDeck.RemoveFromArray(completedDeck);

                aceDeck.PushCardArray(cardArray: completedDeck, isDraggable: false);
                aceDeck.UpdateCardsPosition(false);

                break;
            }

            GameManagerComponent.AddScoreValue(Public.SCORE_MOVE_TO_ACE);
            
            if (AudioCtrl != null)
            {
                AudioCtrl.Play(AudioController.AudioType.MoveToAce);
            }

            srcDeck.UpdateCardsPosition(false);
            srcDeck.UpdateDraggableStatus();
        }
    }
}
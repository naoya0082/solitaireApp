using System;
using System.Collections.Generic;
using System.Linq;
using SimpleSolitaire.Model.Config;
using SimpleSolitaire.Model.Enum;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace SimpleSolitaire.Controller
{
    public enum FreecellAmountType
    {
        FourFreeCell = 4,
        TwoFreeCell = 2,
        OneFreeCell = 1,
    }

    public enum FreecellDifficultyType
    {
        Random = 0,
        Easy = 1,
    }

    public class FreecellCardLogic : CardLogic
    {
        public Deck[] FreecellDeckArray = new Deck[4];
        protected override int CardNums => Public.FREECELL_CARD_NUMS;
        private FreecellAmountType TempFreecellAmountType { get; set; } = FreecellAmountType.FourFreeCell;
        private FreecellDifficultyType TempFreecellDifficulty { get; set; } = FreecellDifficultyType.Random;

        [Space, Header("Freecell logic fields: ")]
        public FreecellAmountType CurrentFreecellAmountType;

        public FreecellDifficultyType CurrentDifficultyType;
        public int DifficultyReplaceAmount = 25;

        [Header("Freecell amount toggles:"), SerializeField]
        private Toggle _oneFreecellToggle;

        [SerializeField] private Toggle _twoFreecellToggle;
        [SerializeField] private Toggle _fourFreecellToggle;
        
        [Header("Freecell mode toggles:")]

        [SerializeField] private Toggle _randomToggle;
        [SerializeField] private Toggle _easyToggle;

        private void ChangeFreecellAmountTypeByToggle(FreecellAmountType type)
        {
            if (CurrentFreecellAmountType == type) return;

            TempFreecellAmountType = type;
        }

        public void InitFreecellToggles()
        {
            TempFreecellDifficulty = CurrentDifficultyType;
            TempFreecellAmountType = CurrentFreecellAmountType;
            
            _oneFreecellToggle.SetIsOnWithoutNotify(CurrentFreecellAmountType == FreecellAmountType.OneFreeCell);
            _twoFreecellToggle.SetIsOnWithoutNotify(CurrentFreecellAmountType == FreecellAmountType.TwoFreeCell);
            _fourFreecellToggle.SetIsOnWithoutNotify(CurrentFreecellAmountType == FreecellAmountType.FourFreeCell);
            _randomToggle.SetIsOnWithoutNotify(CurrentDifficultyType == FreecellDifficultyType.Random);
            _easyToggle.SetIsOnWithoutNotify(CurrentDifficultyType == FreecellDifficultyType.Easy);
              
            _oneFreecellToggle.interactable = CurrentDifficultyType == FreecellDifficultyType.Random;
            _twoFreecellToggle.interactable = CurrentDifficultyType == FreecellDifficultyType.Random;
            _fourFreecellToggle.interactable = CurrentDifficultyType == FreecellDifficultyType.Random;
        }
        
        private void ChangeFreecellDifficultyTypeByToggle(FreecellDifficultyType type)
        {
            if (TempFreecellDifficulty == type) return;

            if (type == FreecellDifficultyType.Easy)
            {
                TempFreecellAmountType = FreecellAmountType.FourFreeCell;

                _oneFreecellToggle.SetIsOnWithoutNotify(false);
                _twoFreecellToggle.SetIsOnWithoutNotify(false);
                _fourFreecellToggle.SetIsOnWithoutNotify(true);
            }
            
            _oneFreecellToggle.interactable = type == FreecellDifficultyType.Random;
            _twoFreecellToggle.interactable = type == FreecellDifficultyType.Random;
            _fourFreecellToggle.interactable = type == FreecellDifficultyType.Random;

            TempFreecellDifficulty = type;
        }

        public override void InitCardLogic()
        {
            InitFreecellToggles();

            base.InitCardLogic();
        }

        protected override void GenerateRandomCardNums()
        {
            switch (CurrentDifficultyType)
            {
                case FreecellDifficultyType.Random:
                {
                    base.GenerateRandomCardNums();
                    break;
                }
                case FreecellDifficultyType.Easy:
                {
                    base.GenerateRandomCardNums();
                    int replaceAmount = DifficultyReplaceAmount;
                    int lastReplaceIndex = 0;
                    for (int i = CardNumberArray.Length - 1; i > 0; i--)
                    {
                        if (replaceAmount <= 0)
                        {
                            break;
                        }

                        if (CardNumberArray[i] % 13 < 6)
                        {
                            replaceAmount--;

                            while (CardNumberArray[lastReplaceIndex] % 13 < 6)
                            {
                                if (lastReplaceIndex > CardNums)
                                {
                                    break;
                                }

                                lastReplaceIndex++;
                                replaceAmount--;
                            }

                            if (replaceAmount <= 0)
                            {
                                break;
                            }

                            int currentCardValue = CardNumberArray[i];
                            int cardForReplaceValue = CardNumberArray[lastReplaceIndex];

                            CardNumberArray[lastReplaceIndex] = currentCardValue;
                            CardNumberArray[i] = cardForReplaceValue;

                            /* Test debug.
                            Debug.LogError($"lastReplaceIndex {lastReplaceIndex} replaceAmount {replaceAmount} Replace {currentCardValue} with {cardForReplaceValue} ");
                            */
                        }
                    }

                    break;
                }
            }
        }

        protected override void InitAllDeckArray()
        {
            List<Deck> allDecks = new List<Deck>();
            int j = 0;
            for (int i = 0; i < AceDeckArray.Length; i++)
            {
                AceDeckArray[i].Type = DeckType.DECK_TYPE_ACE;
                allDecks.Add(AceDeckArray[i]);
            }

            for (int i = 0; i < BottomDeckArray.Length; i++)
            {
                BottomDeckArray[i].Type = DeckType.DECK_TYPE_BOTTOM;
                allDecks.Add(BottomDeckArray[i]);
            }

            var availableFreeCells = (int) CurrentFreecellAmountType;
            for (int i = 0; i < FreecellDeckArray.Length; i++)
            {
                if (i >= availableFreeCells)
                {
                    FreecellDeckArray[i].gameObject.SetActive(false);
                }
                else
                {
                    FreecellDeckArray[i].gameObject.SetActive(true);
                    FreecellDeckArray[i].Type = DeckType.DECK_TYPE_FREECELL;
                    allDecks.Add(FreecellDeckArray[i]);
                }
            }

            AllDeckArray = allDecks.ToArray();

            for (int i = 0; i < AllDeckArray.Length; i++)
            {
                AllDeckArray[i].DeckNum = i;
            }
        }

        public override void InitializeSpacesDictionary()
        {
            base.InitializeSpacesDictionary();

            SpacesDict.Add(DeckSpacesTypes.DECK_SPACE_VERTICAL_BOTTOM_OPENED, DeckHeight / 3.5f);
            SpacesDict.Add(DeckSpacesTypes.DECK_SPACE_VERTICAL_BOTTOM_CLOSED, DeckHeight / 3.5f / 2);
        }

        public override void SubscribeEvents()
        {
            _oneFreecellToggle.onValueChanged.AddListener(delegate
            {
                ChangeFreecellAmountTypeByToggle(FreecellAmountType.OneFreeCell);
            });
            _twoFreecellToggle.onValueChanged.AddListener(delegate
            {
                ChangeFreecellAmountTypeByToggle(FreecellAmountType.TwoFreeCell);
            });
            _fourFreecellToggle.onValueChanged.AddListener(delegate
            {
                ChangeFreecellAmountTypeByToggle(FreecellAmountType.FourFreeCell);
            });
            _randomToggle.onValueChanged.AddListener(delegate
            {
                ChangeFreecellDifficultyTypeByToggle(FreecellDifficultyType.Random);
            });
            _easyToggle.onValueChanged.AddListener(delegate
            {
                ChangeFreecellDifficultyTypeByToggle(FreecellDifficultyType.Easy);
            });
        }

        public override void UnsubscribeEvents()
        {
            _oneFreecellToggle.onValueChanged.RemoveAllListeners();
            _twoFreecellToggle.onValueChanged.RemoveAllListeners();
            _fourFreecellToggle.onValueChanged.RemoveAllListeners();
            _randomToggle.onValueChanged.RemoveAllListeners();
            _easyToggle.onValueChanged.RemoveAllListeners();
        }

        public override void OnNewGameStart()
        {
            CurrentFreecellAmountType = TempFreecellAmountType;
            CurrentDifficultyType = TempFreecellDifficulty;
            InitAllDeckArray();
            IsGameStarted = true;
        }

        public void SetFreecellAmountImmediately(FreecellAmountType type)
        {
            TempFreecellAmountType = type;
            CurrentFreecellAmountType = TempFreecellAmountType;
        }
        
        public void SetFreecellDifficultyImmediately(FreecellDifficultyType type)
        {
            TempFreecellDifficulty = type;
            CurrentDifficultyType = TempFreecellDifficulty;
        }

        /// <summary>
        /// Initialize deck of cards.
        /// </summary>
        protected override void InitDeckCards()
        {
            while (PackDeck.HasCards)
            {
                for (int i = 0; i < BottomDeckArray.Length; i++)
                {
                    int cardCount = i < 4 ? 7 : 6;
                    if (BottomDeckArray[i].CardsCount >= cardCount)
                    {
                        continue;
                    }

                    Deck bottomDeck = BottomDeckArray[i];
                    bottomDeck.PushCard(PackDeck.Pop());
                    bottomDeck.UpdateDraggableStatus();
                    bottomDeck.UpdateCardsPosition(true);
                }
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
                Deck targetDeck = AllDeckArray[i];
                if (targetDeck.Type == DeckType.DECK_TYPE_BOTTOM || targetDeck.Type == DeckType.DECK_TYPE_ACE ||
                    targetDeck.Type == DeckType.DECK_TYPE_FREECELL)
                {
                    if (targetDeck.OverlapWithCard(card))
                    {
                        isHasTarget = true;
                        Deck srcDeck = card.Deck;

                        if (targetDeck.Type == DeckType.DECK_TYPE_BOTTOM)
                        {
                            int cardsForAccept = card.Deck.CardsAmountFromCard(card);
                            int borderForMove = GetSuperMovesAmount();

                            if (cardsForAccept > borderForMove)
                            {
                                continue;
                            }
                        }

                        if (targetDeck.AcceptCard(card))
                        {
                            if (targetDeck.Type == DeckType.DECK_TYPE_FREECELL)
                            {
                                Card topCard = srcDeck.GetTopCard();
                                if (topCard == card)
                                {
                                    WriteUndoState();
                                    targetDeck.PushCard(srcDeck.Pop());
                                    targetDeck.UpdateCardsPosition(false);
                                    srcDeck.UpdateCardsPosition(false);

                                    ActionAfterEachStep();

                                    return;
                                }
                            }
                            else
                            {
                                WriteUndoState();
                                Card[] popCards = srcDeck.PopFromCard(card);
                                targetDeck.PushCardArray(popCards);
                                targetDeck.UpdateCardsPosition(false);
                                srcDeck.UpdateCardsPosition(false);

                                srcDeck.UpdateDraggableStatus();
                                targetDeck.UpdateDraggableStatus();
                                ActionAfterEachStep();

                                if (targetDeck.Type == DeckType.DECK_TYPE_ACE)
                                {
                                    GameManagerComponent.AddScoreValue(Public.SCORE_MOVE_TO_ACE);
                                    if (AudioCtrl != null)
                                    {
                                        AudioCtrl.Play(AudioController.AudioType.MoveToAce);
                                    }
                                }
                                else
                                {
                                    if (AudioCtrl != null)
                                    {
                                        AudioCtrl.Play(AudioController.AudioType.Move);
                                    }
                                }

                                return;
                            }
                        }
                    }
                }
                else
                {
                    isPackWasteNotFound = true;
                }
            }

            if (isPackWasteNotFound &&
                (card.Deck.Type != DeckType.DECK_TYPE_PACK && card.Deck.Type != DeckType.DECK_TYPE_WASTE) ||
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
        }

        protected override void SetPackDeckBg()
        {
        }

        public int GetSuperMovesAmount()
        {
            int freeBottomDecks = AllDeckArray.Count(x => !x.HasCards && x.Type == DeckType.DECK_TYPE_BOTTOM);
            int freeFreecellDecks = AllDeckArray.Count(x => !x.HasCards && x.Type == DeckType.DECK_TYPE_FREECELL);
            return (freeFreecellDecks + 1) * (int) Math.Pow(2, freeBottomDecks);
        }
    }
}
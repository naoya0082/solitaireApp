using System;
using SimpleSolitaire.Model.Config;
using SimpleSolitaire.Model.Enum;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SimpleSolitaire.Controller
{
    
    public enum DeckSpacesTypes
    {
        DECK_SPACE_VERTICAL_BOTTOM_OPENED = 1,
        DECK_SPACE_VERTICAL_BOTTOM_CLOSED = 2,
        DECK_SPACE_HORIONTAL_WASTE = 3,
        DECK_SPACE_VERTICAL_ACE = 4,
        DECK_SPACE_VERTICAL_FREECELL = 5,
        DECK_PACK_HORIZONTAL = 6,
    }
    
    public abstract class CardLogic : MonoBehaviour
    {
        protected Vector3[] Corners;
        [SerializeField]
        protected RectTransform CorrectlyDeck;
        
        public Color DraggableColor;
        public Color NondraggableColor;
        
        public List<HandOrientationElement> DeckElements;

        public Card[] CardsArray;
        public int[] CardNumberArray = new int[52];
        public Deck[] BottomDeckArray = new Deck[7];
        public Deck[] AceDeckArray = new Deck[4];
        public Deck[] AllDeckArray = new Deck[13];
        public Deck WasteDeck;
        public Deck PackDeck;
        public GameManager GameManagerComponent;
        public HintManager HintManagerComponent;
        public AutoCompleteManager AutoCompleteComponent;
        public UndoPerformer UndoPerformerComponent;

        public ParticleSystem ParticleStars;

        private readonly string _packNone = "pack_deck_none";
        private readonly string _packRotate = "pack_deck_rotate";
        private bool _isUserMadeFirstMove;
        private bool _isGameStarted;

        public bool HighlightDraggable { get; set; }
        public bool IsGameStarted { get; protected set; }
        public bool IsNeedResetPack { get; set; }
        public HandOrientation Orientation { get; set; }

        protected abstract int CardNums { get; }
        
        protected AudioController AudioCtrl;

        protected Dictionary<DeckSpacesTypes, float> SpacesDict;
        protected Dictionary<string, Sprite> CachedSprtesDict;
        
        public float DeckWidth => _deckWidth;
        public float DeckHeight => _deckHeight;

        private float _deckWidth;
        private float _deckHeight;

        protected void Awake()
        {
            CachedSprtesDict = new Dictionary<string, Sprite>();
            HighlightDraggable = true;
            InitializeSpacesDictionary();
        }

        protected void Start()
        {
            AudioCtrl = AudioController.Instance;
        }

        /// <summary>
        /// Initialize logic structure of game session.
        /// </summary>
        public virtual void InitCardLogic()
        {
            InitCardNodes();
            InitAllDeckArray();
            SetHandOrientation();
            UndoPerformerComponent.ResetUndoStates();
            ParticleStars.Stop();
        }

        public virtual void InitializeSpacesDictionary()
        {
            SpacesDict = new Dictionary<DeckSpacesTypes, float>();
            Corners = new Vector3[4];
            CorrectlyDeck.GetWorldCorners(Corners);

            _deckHeight = Corners[2].y - Corners[0].y;
            _deckWidth = Corners[2].x - Corners[0].x;
        }

        public float GetSpaceFromDictionary(DeckSpacesTypes type)
        {
            if (SpacesDict == null || !SpacesDict.ContainsKey(type))
            {
                return 0f;
            }

            return SpacesDict[type];
        }

        public abstract void SubscribeEvents();
        public abstract void UnsubscribeEvents();
        
        public abstract void OnNewGameStart();

        private void OnDisable()
        {
            if (HintManagerComponent != null)
            {
                HintManagerComponent.ResetHint();
            }
        }

        /// <summary>
        /// Randomize cards.
        /// </summary>
        protected virtual void GenerateRandomCardNums()
        {
            int cardNums = CardNums;
            int[] tagArray = new int[cardNums];
            int i = 0;
            for (i = 0; i < cardNums; i++)
            {
                tagArray[i] = 0;
            }

            for (i = 0; i < cardNums; i++)
            {
                int rand = Random.Range(0, cardNums);
                while (rand < cardNums && tagArray[rand] == 1)
                {
                    rand = Random.Range(0, cardNums);
                }

                tagArray[rand] = 1;
                CardNumberArray[i] = rand;
            }
        }

        /// <summary>
        /// Randomize cards.
        /// </summary>
        public void InitSpecificCardNums(int[] numsArray)
        {
            CardNumberArray = numsArray;
        }

        public void SetHandOrientation()
        {
            DeckElements.ForEach(x =>
            {
                switch (Orientation)
                {
                    case HandOrientation.LEFT:
                        x.RectRoot.anchorMin = x.LeftTransformRef.anchorMin;
                        x.RectRoot.anchorMax = x.LeftTransformRef.anchorMax;
                        x.RectRoot.pivot = x.LeftTransformRef.pivot;
                        x.RectRoot.localScale = x.LeftTransformRef.localScale;
                        x.RectRoot.anchoredPosition = x.LeftTransformRef.anchoredPosition;
                        x.RectRoot.sizeDelta = x.LeftTransformRef.sizeDelta;
                        break;
                    case HandOrientation.RIGHT:
                        x.RectRoot.anchorMin = x.RightTransformRef.anchorMin;
                        x.RectRoot.anchorMax = x.RightTransformRef.anchorMax;
                        x.RectRoot.pivot = x.RightTransformRef.pivot;
                        x.RectRoot.localScale = x.RightTransformRef.localScale;
                        x.RectRoot.anchoredPosition = x.RightTransformRef.anchoredPosition;
                        x.RectRoot.sizeDelta = x.RightTransformRef.sizeDelta;
                        break;
                }
            });

            for (int i = 0; i < AllDeckArray.Length; i++)
            {
                Deck targetDeck = AllDeckArray[i];
                targetDeck.UpdateCardsPosition(false);
            }
            HintManagerComponent.UpdateAvailableForDragCards();
        }

        /// <summary>
        /// Initialize cards in the game.
        /// </summary>
        private void InitCardNodes()
        {
            int cardNums = CardNums;
            for (int i = 0; i < cardNums; i++)
            {
                CardsArray[i].transform.SetParent(transform);
                CardsArray[i].InitWithNumber(i);
                CardsArray[i].CardLogicComponent = this;
            }
        }

        /// <summary>
        /// Initialize deck of cards.
        /// </summary>
        protected abstract void InitDeckCards();

        /// <summary>
        /// Initialize deck array.
        /// </summary>
        protected virtual void InitAllDeckArray()
        {
            int j = 0;
            for (int i = 0; i < AceDeckArray.Length; i++)
            {
                AceDeckArray[i].Type = DeckType.DECK_TYPE_ACE;
                AllDeckArray[j++] = AceDeckArray[i];
            }

            for (int i = 0; i < BottomDeckArray.Length; i++)
            {
                BottomDeckArray[i].Type = DeckType.DECK_TYPE_BOTTOM;
                AllDeckArray[j++] = BottomDeckArray[i];
            }

            if (WasteDeck != null)
            {
                WasteDeck.Type = DeckType.DECK_TYPE_WASTE;
                AllDeckArray[j++] = WasteDeck;
            }

            if (PackDeck != null)
            {
                PackDeck.Type = DeckType.DECK_TYPE_PACK;
                AllDeckArray[j++] = PackDeck;
            }

            for (int i = 0; i < AllDeckArray.Length; i++)
            {
                AllDeckArray[i].DeckNum = i;
            }   
        }

        /// <summary>
        /// Call when we drop card.
        /// </summary>
        /// <param name="card">Dropped card</param>
        public abstract void OnDragEnd(Card card);

        /// <summary>
        /// Call when we click on pack with cards.
        /// </summary>
        public abstract void OnClickPack();

        /// <summary>
        /// Hide all cards from waste to pack.
        /// </summary>
        protected void MoveFromWasteToPack()
        {
            int cardNums = WasteDeck.CardsCount;
            for (int i = 0; i < cardNums; i++)
            {
                PackDeck.PushCard(WasteDeck.Pop());
            }

            PackDeck.UpdateCardsPosition(false);
            WasteDeck.UpdateCardsPosition(false);
            
            if (AudioCtrl != null)
            {
                AudioCtrl.Play(AudioController.AudioType.MoveToPack);
            }
        }

        /// <summary>
        /// Check for player win or no.
        /// </summary>
        public void CheckWinGame()
        {
            bool hasWin = true;
            for (int i = 0; i < AceDeckArray.Length; i++)
            {
                if (AceDeckArray[i].CardsCount != Public.CARD_NUMS_OF_SUIT)
                {
                    hasWin = false;
                    break;
                }
            }

            if (hasWin)
            {
                GameManagerComponent.HasWinGame();
                IsGameStarted = false;
            }
        }

        /// <summary>
        /// Call after each step.
        /// </summary>
        public void ActionAfterEachStep()
        {
            if (!_isUserMadeFirstMove)
            {
                _isUserMadeFirstMove = true;
            }

            SetPackDeckBg();
            GameManagerComponent.CardMove();

            if (AutoCompleteComponent.IsAutoCompleteActive)
            {
                HintManagerComponent.UpdateAvailableForAutoCompleteCards();
            }
            else
            {
                HintManagerComponent.UpdateAvailableForDragCards();
            }

            CheckWinGame();
        }

        /// <summary>
        /// Shuffle cards by state type.
        /// </summary>
        /// <param name="bReplay">Replay game or start new</param>
        public void Shuffle(bool bReplay)
        {
            HintManagerComponent.IsHintWasUsed = false;
            IsNeedResetPack = false;
            GameManagerComponent.RestoreInitialState();
            RestoreInitialState();

            if (!bReplay)
            {
                GenerateRandomCardNums();
            }

            int cardNums = CardNums;
            for (int i = 0; i < cardNums; i++)
            {
                Card card = CardsArray[i];
                card.InitWithNumber(CardNumberArray[i]);
#if UNITY_EDITOR
                string cardName = $"{card.GetTypeName()}_{card.Number} Index: ({card.CardNumber})"; 
                card.gameObject.name = $"CardHolder ({cardName})";
                card.BackgroundImage.gameObject.name = $"Card_{cardName}";
#endif
                PackDeck.PushCard(card);
            }

            InitDeckCards();
            SetPackDeckBg();
            HintManagerComponent.UpdateAvailableForDragCards();
            IsGameStarted = true;
        }

        /// <summary>
        /// Initialize default state of game.
        /// </summary>
        public void RestoreInitialState()
        {
            for (int i = 0; i < AllDeckArray.Length; i++)
            {
                AllDeckArray[i].RestoreInitialState();
            }
        }

        /// <summary>
        /// Set up background of pack.
        /// </summary>
        protected virtual void SetPackDeckBg()
        {
            string name = _packNone;
            if (PackDeck != null && PackDeck.HasCards || WasteDeck != null && WasteDeck.HasCards)
            {
                name = _packRotate;
            }

            PackDeck.SetBackgroundImg(name);
        }

        /// <summary>
        /// Write state of current decks and cards.
        /// </summary>
        public void WriteUndoState(bool isTemp = false)
        {
            UndoPerformerComponent.AddUndoState(AllDeckArray, isTemp);
            UndoPerformerComponent.ActivateUndoButton();
        }

        /// <summary>
        /// If user made any changes with cards.
        /// </summary>
        public bool IsMoveWasMadeByUser()
        {
            return _isUserMadeFirstMove;
        }

        public Sprite LoadSprite(string path)
        {
            if(CachedSprtesDict.ContainsKey(path))
            {
                return CachedSprtesDict[path];
            }
            else
            {
                Sprite sprite = Resources.Load<Sprite>(path);
                
                CachedSprtesDict.Add(path, sprite);

                return sprite;
            }
        }

        public void SaveGameState(bool isTempState)
        {
            WriteUndoState(isTempState);
            UndoPerformerComponent.SaveGame(
                time: GameManagerComponent.TimeCount,
                steps: GameManagerComponent.StepCount,
                score: GameManagerComponent.ScoreCount
                );
        }
    }
}
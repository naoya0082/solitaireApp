using SimpleSolitaire.Model.Enum;
using System.Collections;
using SimpleSolitaire.Model.Config;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimpleSolitaire.Controller
{
    public abstract class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public int CardType = 0;
        public int CardNumber = 0;
        public int Number = 0;
        public int CardStatus = 0;
        public int CardColor = 0;
        public bool IsDraggable = false;

        public int IndexZ;

        public CardLogic CardLogicComponent;
        public Image BackgroundImage;
        public RectTransform CardRect;

        private Vector3 _lastMousePosition = Vector3.zero;
        private Vector3 _offset;
        private IEnumerator _coroutine;
        private IEnumerator _doubleClickTimer;

        private Vector3 _newPosition;

        private bool _isDragging;
        private Deck _deck;

        private string _cachedSpritePath = string.Empty;

        public Deck Deck
        {
            get { return _deck; }
            set { _deck = value; }
        }

        private void Start()
        {
            IndexZ = transform.GetSiblingIndex();
        }

        /// <summary>
        /// Set new background image for card.
        /// </summary>
        /// <param name="path"></param>
        protected void SetBackgroundImg(string path)
        {
            if (_cachedSpritePath == path)
            {
                return;
            }

            _cachedSpritePath = path;

            Sprite tempType = CardLogicComponent.LoadSprite(path);
            BackgroundImage.sprite = tempType;
        }

        /// <summary>
        /// Show star particles.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ActivateParticle()
        {
            yield return new WaitForSeconds(0.1f);
            CardLogicComponent.ParticleStars.Play();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (CardLogicComponent.AutoCompleteComponent.IsAutoCompleteActive || !IsDraggable)
            {
                return;
            }

            _isDragging = true;

            IndexZ = transform.GetSiblingIndex();
            _deck.SetCardsToTop(this);
            CardLogicComponent.ParticleStars.transform.SetParent(gameObject.transform);
            CardLogicComponent.ParticleStars.transform.SetAsFirstSibling();

            _coroutine = ActivateParticle();
            StartCoroutine(_coroutine);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (CardLogicComponent.AutoCompleteComponent.IsAutoCompleteActive || !IsDraggable)
            {
                return;
            }

            RectTransformUtility.ScreenPointToWorldPointInRectangle(CardRect, Input.mousePosition,
                eventData.enterEventCamera, out _newPosition);
            if (_lastMousePosition != Vector3.zero)
            {
                Vector3 offset = _newPosition - _lastMousePosition;
                transform.position += offset;
                CardLogicComponent.ParticleStars.transform.position = new Vector3(transform.position.x,
                    transform.position.y - 20f, transform.position.z);
                _deck.SetPositionFromCard(this, transform.position.x, transform.position.y);
            }

            _lastMousePosition = _newPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (CardLogicComponent.AutoCompleteComponent.IsAutoCompleteActive || !IsDraggable)
            {
                return;
            }

            transform.SetSiblingIndex(IndexZ);
            _lastMousePosition = Vector3.zero;

            if (_coroutine != null)
                StopCoroutine(_coroutine);
            CardLogicComponent.ParticleStars.Stop();

            CardLogicComponent.OnDragEnd(this);
            _deck.UpdateCardsPosition(false);

            _isDragging = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isDragging)
            {
                return;
            }

            switch (Deck.Type)
            {
                case DeckType.DECK_TYPE_PACK:
                {
                    if (CardLogicComponent.AutoCompleteComponent.IsAutoCompleteActive)
                    {
                        return;
                    }

                    Deck.OnPointerClick(eventData);
                }
                    break;
                case DeckType.DECK_TYPE_BOTTOM:
                case DeckType.DECK_TYPE_WASTE:
                case DeckType.DECK_TYPE_ACE:
                case DeckType.DECK_TYPE_FREECELL:
                {
                    OnTapToPlace();
                } 
                    break;

            }
        }

        /// <summary>
        /// Get card texture by type.
        /// </summary>
        /// <returns> Texture string type</returns>
        protected string GetTexture()
        {
            CardVisualData visualData = CardShirtManager.Instance.VisualData;
            
            return  CardStatus == 0 
                ? $"{Public.PATH_TO_CARD_BACKS_IN_RESOURCES}{visualData.Back}" 
                : $"{Public.PATH_TO_CARD_FRONTS_IN_RESOURCES}{visualData.Front}/{GetTypeName()}{Number}";
        }

        /// <summary>
        /// Set default card status and background image <see cref="SetBackgroundImg"/>.
        /// </summary>
        public void RestoreBackView()
        {
            CardStatus = 0;
            var path = GetTexture();
            SetBackgroundImg(path);
        }

        /// <summary>
        /// Set card position.
        /// </summary>
        /// <param name="position">New card position.</param>
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        /// <summary>
        /// Initialize card by number.
        /// </summary>
        /// <param name="cardNum">Card number.</param>
        public abstract void InitWithNumber(int cardNum);

        /// <summary>
        /// Update card background <see cref="SetBackgroundImg"/>.
        /// </summary>
        public void UpdateCardImg()
        {
            var path = GetTexture();
            SetBackgroundImg(path);
        }

        public string GetTypeName()
        {
            switch (CardType)
            {
                case 0:
                    return Public.SpadeTextureName;
                case 1:
                    return Public.HeartTextureName;
                case 2:
                    return Public.ClubTextureName;
                case 3:
                    return Public.DiamondTextureName;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        ///Called when user click on card double times in specific interval
        /// </summary>
        protected abstract void OnTapToPlace();

        public void SetBackgroundColor(Color color)
        {
            BackgroundImage.color = color;
        }
    }
}
using System.Collections.Generic;
using SimpleSolitaire.Model.Config;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleSolitaire.Controller
{
    public class CardVisualData
    {
        public string Back;
        public string Front;

        public CardVisualData(string back, string front)
        {
            Back = back;
            Front = front;
        }
    }

    [System.Serializable]
    public class GameVisual
    {
        public List<Sprite> Previews;

        public VisualiseElement Current;
        public List<VisualiseElement> Elements;

        public string VisualName;
        public string DefaultName;
        public string SaveName;
    }

    public class CardShirtManager : MonoBehaviour
    {
        public static CardShirtManager Instance;

        public CardVisualData VisualData => new CardVisualData(back: CardBackVisual.VisualName, front: CardFrontVisual.VisualName);

        [Header("BackGrounds:")] public GameVisual BackgroundVisual;
        [Header("Card back:")] public GameVisual CardBackVisual;
        [Header("Card front:")] public GameVisual CardFrontVisual;
        
        public Image GameBG;
        public CardLogic CLComponent;

        private readonly string _animationActionTrigger = "Action";
        private readonly string _animationNonActionTrigger = "NonAction";

        private void Awake()
        {
            Instance = this;
            InitializeButtons();
            SetImageObjects();
            GetSettings();
        }

        private void InitializeButtons()
        {
            CardBackVisual.Elements.ForEach(x => x.Btn.onClick.AddListener(() => SetShirtForCards(x)));
            BackgroundVisual.Elements.ForEach(x => x.Btn.onClick.AddListener(() => SetBackGround(x)));
            CardFrontVisual.Elements.ForEach(x => x.Btn.onClick.AddListener(() => SetFront(x)));
        }

        /// <summary>
        /// Get settings values from player prefs.
        /// </summary>
        private void GetSettings()
        {
            GetVisualSettings(BackgroundVisual);
            GetVisualSettings(CardBackVisual);
            GetVisualSettings(CardFrontVisual);
        }

        /// <summary>
        /// Get visual data save from prefs.
        /// </summary>
        private void GetVisualSettings(GameVisual visual)
        {
            if (PlayerPrefs.HasKey(visual.SaveName))
            {
                visual.VisualName = PlayerPrefs.GetString(visual.SaveName);
            }
            else
            {
                visual.VisualName = visual.DefaultName;
                PlayerPrefs.SetString(visual.SaveName, visual.DefaultName);
            }
        }

        private void Start()
        {
            SetBackGround(BackgroundVisual.Elements.Find(y => y.name == BackgroundVisual.VisualName));
            SetShirtForCards(CardBackVisual.Elements.Find(y => y.name == CardBackVisual.VisualName));
            SetFront(CardFrontVisual.Elements.Find(y => y.name == CardFrontVisual.VisualName));
        }

        /// <summary>
        /// Apply settings for game.
        /// </summary>
        public void SetSettings()
        {
            ActionWithAnimationForObjects(BackgroundVisual);
            ActionWithAnimationForObjects(CardBackVisual);
            ActionWithAnimationForObjects(CardFrontVisual);
        }

        /// <summary>
        /// Set up background of game.
        /// </summary>
        public void SetFront(VisualiseElement element)
        {
            CardFrontVisual.Current = element;
            CardFrontVisual.VisualName = CardFrontVisual.Current.ElementName;
            PlayerPrefs.SetString(CardFrontVisual.SaveName, CardFrontVisual.VisualName);

            ActionWithAnimationForObjects(CardFrontVisual);

            foreach (var item in CLComponent.CardsArray)
            {
                item.UpdateCardImg();
            }
        }

        /// <summary>
        /// Set up background of game.
        /// </summary>
        public void SetBackGround(VisualiseElement element)
        {
            BackgroundVisual.Current = element;
            BackgroundVisual.VisualName = BackgroundVisual.Current.ElementName;
            PlayerPrefs.SetString(BackgroundVisual.SaveName, BackgroundVisual.VisualName);

            ActionWithAnimationForObjects(BackgroundVisual);

            GameBG.sprite = CLComponent.LoadSprite(Public.PATH_TO_BG_IN_RESOURCES + BackgroundVisual.VisualName);
        }

        /// <summary>
        /// Set up shirt for card objects.
        /// </summary>
        /// <param name="shirtObject">Shirt image component.</param>
        public void SetShirtForCards(VisualiseElement element)
        {
            CardBackVisual.Current = element;
            CardBackVisual.VisualName = CardBackVisual.Current.ElementName;
            PlayerPrefs.SetString(CardBackVisual.SaveName, CardBackVisual.VisualName);

            ActionWithAnimationForObjects(CardBackVisual);

            foreach (var item in CLComponent.CardsArray)
            {
                item.UpdateCardImg();
            }
        }

        /// <summary>
        /// Activate animation which highlight chosen background <see cref="currentBackground"/>.
        /// </summary>
        /// <param name="curList"></param>
        /// <param name="currentBackground">Current chosen background</param>
        /// <param name="_name">Name of shirt or background</param>
        private void ActionWithAnimationForObjects(GameVisual visual)
        {
            visual.Elements.ForEach(a =>
            {
                if (a.name == visual.VisualName)
                {
                    a.ActivateCheckmark();
                    a.Anim.enabled = true;
                    a.Anim.speed = 1f;
                    a.Anim.SetTrigger(_animationActionTrigger);
                }
                else
                {
                    a.DeactivateCheckmark();
                    a.Anim.enabled = false;
                    a.transform.localRotation = Quaternion.identity;
                }
            });
        }

        /// <summary>
        /// Set up image components.
        /// </summary>
        private void SetImageObjects()
        {
            InitVisual(BackgroundVisual);
            InitVisual(CardBackVisual);
            InitVisual(CardFrontVisual);
        }

        /// <summary>
        /// Initialize visual elements.
        /// </summary>
        private void InitVisual(GameVisual visual)
        {
            for (int i = 0; i < visual.Elements.Count; i++)
            {
                var visualElement = visual.Elements[i];
                var preview = visual.Previews[i];

                visualElement.VisualImage.sprite = preview;
                visualElement.name = preview.name;
                visualElement.ElementName = preview.name;
            }
        }
    }
}
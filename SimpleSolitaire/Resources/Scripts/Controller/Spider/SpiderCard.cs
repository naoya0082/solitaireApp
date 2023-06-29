using SimpleSolitaire.Model.Config;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    public class SpiderCard : Card
    {
        /// <summary>
        /// Initialize card by number.
        /// </summary>
        /// <param name="cardNum">Card number.</param>
        public override void InitWithNumber(int cardNum)
        {
            var spiderLogic = CardLogicComponent as SpiderCardLogic;
            CardNumber = cardNum;

            switch (spiderLogic.CurrentSpiderSuitsType)
            {
                case SpiderSuitsType.OneSuit:
                {
                    CardType = 0;
                    CardColor = 0;
                }
                    break;

                case SpiderSuitsType.TwoSuits:
                {
                    CardType = Mathf.FloorToInt(cardNum / Public.CARD_NUMS_OF_SUIT) % 2;

                    if (CardType == 1)
                    {
                        CardColor = 1;
                    }
                    else
                    {
                        CardColor = 0;
                    }
                }
                    break;

                case SpiderSuitsType.FourSuits:
                {
                    CardType = Mathf.FloorToInt(cardNum / Public.CARD_NUMS_OF_SUIT) % 4;

                    if (CardType == 1 || CardType == 3)
                    {
                        CardColor = 1;
                    }
                    else
                    {
                        CardColor = 0;
                    }
                }
                    break;
            }

            Number = (cardNum % Public.CARD_NUMS_OF_SUIT) + 1;
            CardStatus = 0;

            var path = GetTexture();
            SetBackgroundImg(path);
        }

        /// <summary>
        ///Called when user click on card double times in specific interval
        /// </summary>
        protected override void OnTapToPlace()
        {
            CardLogicComponent.HintManagerComponent.HintAndSetByClick(this);
        }
    }
}
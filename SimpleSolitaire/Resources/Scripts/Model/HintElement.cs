using SimpleSolitaire.Controller;
using UnityEngine;

namespace SimpleSolitaire.Model
{
    [System.Serializable]
    public class HintElement
    {
        public Card HintCard;
        public Deck DestinationDeck;
        public Vector3 FromPosition;
        public Vector3 ToPosition;

        public HintElement(Card hintCard, Vector3 fromPosition, Vector3 toPosition, Deck destinationDeck)
        {
            HintCard = hintCard;
            FromPosition = fromPosition;
            ToPosition = toPosition;
            DestinationDeck = destinationDeck;
        }
    }
}
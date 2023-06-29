namespace SimpleSolitaire.Model.Config
{
    public class Public
    {
        public static string PATH_TO_CARD_FRONTS_IN_RESOURCES = "Sprites/Cards/";
        public static string PATH_TO_CARD_BACKS_IN_RESOURCES = "Sprites/Cards/Backs/";
        public static string PATH_TO_DECKS_IN_RESOURCES = "Sprites/Decks/";
        public static string PATH_TO_BG_IN_RESOURCES = "Sprites/Backgrounds/";
        
        public static string SpadeTextureName = "spade";
        public static string DiamondTextureName = "diamond";
        public static string ClubTextureName = "club";
        public static string HeartTextureName = "heart";
        
        public static int CARD_NUMS_OF_SUIT = 13;

        public static int KLONDIKE_CARD_NUMS = 52;
        public static int FREECELL_CARD_NUMS = 52;
        public static int SPIDER_CARD_NUMS = 104;

        public static int SCORE_NUMBER = 600000;

        public static int SCORE_MOVE_TO_ACE = 10;
        public static int SCORE_MOVE_TO = 10;
        public static int SCORE_OVER_THIRTY_SECONDS_DECREASE = -30;
    }
}
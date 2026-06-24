namespace Serbull.GameAssets
{
    public static class Services
    {
        //internal
        public static UIManager UI;
        public static Rarity.RarityService Rarity;
        public static Roulette.RouletteService Roulette;
        public static ILocalizationService Localization = new Localization.EmptyService();
        public static IAudioService Audio;
        public static IPlaytimeGiftService PlaytimeGift;
        public static Interact.InteractService InteractService;

        //external
        public static IResourceGiver ResourceGiver;
        public static IPurchaseService Purchase;
    }
}

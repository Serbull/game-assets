namespace Serbull.GameAssets
{
    public static class Services
    {
        public static UIManager UI;
        public static Rarity.RarityService Rarity;
        public static ILocalizationService Localization = new Localization.EmptyService();
        public static IAudioService Audio;
        public static IPlaytimeGiftService PlaytimeGift;
        public static IRouletteService Roulette;
        public static Interact.InteractService InteractService;
    }
}

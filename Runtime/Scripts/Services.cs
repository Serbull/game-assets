using System;

namespace Serbull.GameAssets
{
    public static class Services
    {
        //internal
        public static UIManager UI;
        public static Rarity.RarityService Rarity;
        public static Roulette.RouletteService Roulette;
        public static ILocalizationService Localization = new Localization.EmptyService();
        public static AudioService Audio;
        public static IPlaytimeGiftService PlaytimeGift;
        public static DailyRewardService DailyRewardService;
        public static Interact.InteractService InteractService;

        //external
        public static IResourceGiver ResourceGiver;
        public static IPurchaseService Purchase;

        public static bool IsInitialized { get; private set; }
        public static event Action OnInitialized;

        public static void MarkAsInitialized()
        {
            if (IsInitialized) return;
            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        public static void ResetSceneScopes()
        {
            IsInitialized = false;
        }
    }
}

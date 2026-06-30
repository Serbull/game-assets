using System;

namespace Serbull.GameAssets
{
    public static class Services
    {
        //internal
        public static UIManager UI;
        public static RarityService Rarity;
        public static RouletteService Roulette;
        public static ILocalizationService Localization = new Localization.EmptyService();
        public static AudioService Audio;
        public static PlaytimeRewardService PlaytimeReward;
        public static DailyRewardService DailyReward;
        public static Interact.InteractService Interact;

        //external
        public static IPetService PetService;
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

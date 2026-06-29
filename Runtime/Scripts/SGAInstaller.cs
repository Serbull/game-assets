using System.Linq;
using UnityEngine;

namespace Serbull.GameAssets
{
    [ExecuteInEditMode]
    public class SGAInstaller : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _useAudio;
        [SerializeField] private bool _usePlaytimeReward;
        [SerializeField] private bool _useDailyReward;
        [SerializeField] private bool _useRoulette;
        [SerializeField] private bool _useInteract;
        [Header("Configs")]
        [ReadOnly, SerializeField] private Localization.LocalizationConfig _localizationConfig;
        [ReadOnly, SerializeField] private Rarity.RarityConfig _rarityConfig;
        [ShowIf(nameof(_useAudio)), ReadOnly] public Audio.AudioConfig AudioConfig;
        [ShowIf(nameof(_usePlaytimeReward)), ReadOnly] public PlaytimeReward.GiftConfig PlaytimeGiftConfig;
        [ShowIf(nameof(_useDailyReward)), ReadOnly] public DailyReward.RewardConfig DailyRewardConfig;
        [ShowIf(nameof(_useRoulette)), ReadOnly] public Roulette.RouletteConfig RouletteConfig;

        [Header("UI")]
        [ShowIf(nameof(_usePlaytimeReward))] public PlaytimeReward.GiftPopup PlaytimeRewardPopup;
        [ShowIf(nameof(_useDailyReward))] public DailyReward.RewardPopup DailyRewardPopup;
        [ShowIf(nameof(_useRoulette))] public Roulette.RoulettePopup RoulettePopup;
        public RewardPreviewPopup RewardPreviewPopup;
        public Notification Notification;
        [ShowIf(nameof(_useInteract))] public Interact.InteractButton InteractButton;

#if UNITY_EDITOR
        private void OnEnable()
        {
            UpdateConfigs();
        }

        private void OnValidate()
        {
            UpdateConfigs();
        }

        private void UpdateConfigs()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (_localizationConfig == null)
            {
                _localizationConfig = ConfigProvider.LoadConfig<Localization.LocalizationConfig>(ConfigProvider.ConfigType.Localization);
            }

            if (_rarityConfig == null)
            {
                _rarityConfig = ConfigProvider.LoadConfig<Rarity.RarityConfig>(ConfigProvider.ConfigType.Rarity);
            }

            if (_useAudio && AudioConfig == null)
            {
                AudioConfig = ConfigProvider.LoadConfig<Audio.AudioConfig>(ConfigProvider.ConfigType.Audio);
            }

            if (_usePlaytimeReward && PlaytimeGiftConfig == null)
            {
                PlaytimeGiftConfig = ConfigProvider.LoadConfig<PlaytimeReward.GiftConfig>(ConfigProvider.ConfigType.PlaytimeReward);
            }

            if (_useDailyReward && DailyRewardConfig == null)
            {
                DailyRewardConfig = ConfigProvider.LoadConfig<DailyReward.RewardConfig>(ConfigProvider.ConfigType.DailyReward);
            }

            if (_useRoulette && RouletteConfig == null)
            {
                RouletteConfig = ConfigProvider.LoadConfig<Roulette.RouletteConfig>(ConfigProvider.ConfigType.Roulette);
            }
        }
#endif

        public void Init(IResourceGiver resourceGiver, IPurchaseService purchaseService, Roulette.RouletteData rouletteData, DailyReward.SaveData dailyRewardSaveData, bool isMobileDevice, string language = "en")
        {
            //Purchase
            Services.ResourceGiver = resourceGiver;
            Services.Purchase = purchaseService;

            //Localization
            if (Services.Localization.GetType() == typeof(Localization.EmptyService))
            {
                var emptyService = Services.Localization as Localization.EmptyService;
                var localizationManager = new Localization.LocalizationManager(_localizationConfig, language);
                Services.Localization = localizationManager;

                if (emptyService.Localizations.Count > 0)
                {
                    localizationManager.AddLocalization(emptyService.Localizations.ToArray());
                }

                EventBus.Publish(new Localization.UpdateLocalizationEvent());
            }

            //UI
            var uiService = new UIManager(RewardPreviewPopup, Notification, PlaytimeRewardPopup, DailyRewardPopup, RoulettePopup, InteractButton);
            Services.UI = uiService;

            //Rare
            if (Services.Rarity == null)
            {
                var rarityService = new Rarity.RarityService(_rarityConfig);
                Services.Rarity = rarityService;
                //Add localizations
                Services.Localization.AddLocalization(_rarityConfig.Localizations);
            }

            //Audio
            if (_useAudio)
            {
                var audioManager = new GameObject("AudioManager").AddComponent<Audio.AudioManager>();
                audioManager.Init(AudioConfig);
                Services.Audio = audioManager;
            }

            //Playtime Reward
            if (_usePlaytimeReward)
            {
                var playtimeGiftManager = new PlaytimeReward.GiftManager(PlaytimeGiftConfig, resourceGiver);
                Services.PlaytimeGift = playtimeGiftManager;
            }

            //Daily Reward
            if (_useDailyReward)
            {
                var dailyRewardService = new DailyRewardService(DailyRewardConfig, dailyRewardSaveData);
                Services.DailyRewardService = dailyRewardService;
            }

            //Roulette
            if (_useRoulette)
            {
                var rouletteManager = new Roulette.RouletteService(RouletteConfig, resourceGiver, rouletteData);
                Services.Roulette = rouletteManager;
            }

            //Interact
            if (_useInteract)
            {
                var interactService = new Interact.InteractService(Camera.main.transform);
                Services.InteractService = interactService;
                InteractButton.SetMobile(isMobileDevice);
                InteractButton.Hide();
            }
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (_useRoulette)
            {
                Services.Roulette.Tick();
            }

            if (_useInteract)
            {
                Services.InteractService.Update();
            }
        }
    }
}

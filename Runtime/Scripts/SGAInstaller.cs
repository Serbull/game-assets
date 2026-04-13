using System.Linq;
using UnityEngine;

namespace Serbull.GameAssets
{
    [ExecuteInEditMode]
    public class SGAInstaller : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _useAudio;
        [SerializeField] private bool _usePlaytimeGift;
        [SerializeField] private bool _useRoulette;
        [SerializeField] private bool _useInteract;
        [Header("Configs")]
        [ReadOnly] public Localization.LocalizationConfig LocalizationConfig;
        [ReadOnly] public Rare.RareConfig RareConfig;
        [ShowIf(nameof(_useAudio)), ReadOnly] public Audio.AudioConfig AudioConfig;
        [ShowIf(nameof(_usePlaytimeGift)), ReadOnly] public PlaytimeGift.GiftConfig PlaytimeGiftConfig;
        [ShowIf(nameof(_useRoulette)), ReadOnly] public Roulette.RouletteConfig RouletteConfig;

        [Header("UI")]
        [ShowIf(nameof(_usePlaytimeGift))] public PlaytimeGift.GiftPopup PlaytimeGiftPopup;
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

            if (LocalizationConfig == null)
            {
                LocalizationConfig = ConfigProvider.LoadConfig<Localization.LocalizationConfig>(ConfigProvider.ConfigType.Localization);
            }

            if (RareConfig == null)
            {
                RareConfig = ConfigProvider.LoadConfig<Rare.RareConfig>(ConfigProvider.ConfigType.Rare);
            }

            if (_useAudio && AudioConfig == null)
            {
                AudioConfig = ConfigProvider.LoadConfig<Audio.AudioConfig>(ConfigProvider.ConfigType.Audio);
            }

            if (_usePlaytimeGift && PlaytimeGiftConfig == null)
            {
                PlaytimeGiftConfig = ConfigProvider.LoadConfig<PlaytimeGift.GiftConfig>(ConfigProvider.ConfigType.PlaytimeGift);
            }

            if (_useRoulette && RouletteConfig == null)
            {
                RouletteConfig = ConfigProvider.LoadConfig<Roulette.RouletteConfig>(ConfigProvider.ConfigType.Roulette);
            }
        }
#endif

        public void Init(IResourceGiver resourceGiver, Roulette.RouletteData rouletteData, bool isMobileDevice, string language = "en")
        {
            //UI
            var uiService = new UIManager(RewardPreviewPopup, Notification, PlaytimeGiftPopup, RoulettePopup, InteractButton);
            Services.UI = uiService;

            //Rare
            if (Services.Rare == null)
            {
                var rareManager = new Rare.RareManager(RareConfig);
                Services.Rare = rareManager;
            }

            //Localization
            if (Services.Localization.GetType() == typeof(Localization.EmptyService))
            {
                var localizationManager = new Localization.LocalizationManager(LocalizationConfig, language);
                Services.Localization = localizationManager;

                var localizations = (Services.Localization as Localization.EmptyService).Localizations;
                if (localizations.Count > 0)
                {
                    localizationManager.AddLocalization(localizations.ToArray());
                }

                EventBus.Publish(new Localization.UpdateLocalizationEvent());
            }

            //Audio
            if (_useAudio)
            {
                var audioManager = new GameObject("AudioManager").AddComponent<Audio.AudioManager>();
                audioManager.Init(AudioConfig);
                Services.Audio = audioManager;
            }

            //Playtime Gift
            if (_usePlaytimeGift)
            {
                var playtimeGiftManager = new PlaytimeGift.GiftManager(PlaytimeGiftConfig, resourceGiver);
                Services.PlaytimeGift = playtimeGiftManager;
            }

            //Roulette
            if (_useRoulette)
            {
                var rouletteManager = new Roulette.RouletteManager(RouletteConfig, resourceGiver, rouletteData);
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
                Services.Roulette.Update();
            }

            if (_useInteract)
            {
                Services.InteractService.Update();
            }
        }
    }
}

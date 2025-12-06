using UnityEngine;

namespace Serbull.GameAssets
{
    [ExecuteInEditMode]
    public class SGAInstaller : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _usePlaytimeGift;
        [SerializeField] private bool _useAudio;
        [Header("Configs")]
        [ReadOnly] public Localization.LocalizationConfig LocalizationConfig;
        [ReadOnly] public Rare.RareConfig RareConfig;
        [ShowIf(nameof(_usePlaytimeGift)), ReadOnly] public PlaytimeGift.GiftConfig PlaytimeGiftConfig;
        [ShowIf(nameof(_useAudio)), ReadOnly] public Audio.AudioConfig AudioConfig;

        [Header("UI")]
        [ShowIf(nameof(_usePlaytimeGift))] public PlaytimeGift.GiftPopup PlaytimeGiftPopup;
        public RewardPreviewPopup RewardPreviewPopup;
        public Notification Notification;

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

            if (_usePlaytimeGift && PlaytimeGiftConfig == null)
            {
                PlaytimeGiftConfig = ConfigProvider.LoadConfig<PlaytimeGift.GiftConfig>(ConfigProvider.ConfigType.PlaytimeGift);
            }

            if (_useAudio && AudioConfig == null)
            {
                AudioConfig = ConfigProvider.LoadConfig<Audio.AudioConfig>(ConfigProvider.ConfigType.Audio);
            }
        }
#endif

        public void Init(IResourceGiver resourceGiver, string language = "en")
        {
            //UI
            var uiService = new UIManager(RewardPreviewPopup, Notification, PlaytimeGiftPopup);
            Services.UI = uiService;

            //Rare
            if (Services.Rare == null)
            {
                var rareManager = new Rare.RareManager(RareConfig);
                Services.Rare = rareManager;
            }

            //Localization
            if (Services.Localization == null)
            {
                var localizationManager = new Localization.LocalizationManager(LocalizationConfig, language);
                Services.Localization = localizationManager;
            }

            //Playtime Gift
            if (_usePlaytimeGift && Services.PlaytimeGift == null)
            {
                var playtimeGiftManager = new PlaytimeGift.GiftManager(PlaytimeGiftConfig, resourceGiver);
                Services.PlaytimeGift = playtimeGiftManager;
            }

            //Audio
            if (_useAudio && Services.Audio == null)
            {
                var audioManager = new GameObject("AudioManager").AddComponent<Audio.AudioManager>();
                audioManager.Init(AudioConfig);
                Services.Audio = audioManager;
            }
        }
    }
}

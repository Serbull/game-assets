using UnityEngine;

namespace Serbull.GameAssets
{
    [ExecuteInEditMode]
    public class SGAInstaller : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _usePlaytimeGift;
        [Header("Configs")]
        [ReadOnly] public Localization.LocalizationConfig LocalizationConfig;
        [ReadOnly] public Rare.RareConfig RareConfig;
        [ShowIf(nameof(_usePlaytimeGift)), ReadOnly] public PlaytimeGift.GiftConfig PlaytimeGiftConfig;

        [Header("UI")]
        [ShowIf(nameof(_usePlaytimeGift))] public PlaytimeGift.GiftPopup PlaytimeGiftPopup;
        public RewardPreviewPopup RewardPreviewPopup;
        public Notification Notification;

#if UNITY_EDITOR
        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                return;
            }

            LocalizationConfig = ConfigProvider.LoadConfig<Localization.LocalizationConfig>(ConfigProvider.ConfigType.Localization);
            RareConfig = ConfigProvider.LoadConfig<Rare.RareConfig>(ConfigProvider.ConfigType.Rare);

            if (_usePlaytimeGift && PlaytimeGiftConfig == null)
            {
                PlaytimeGiftConfig = ConfigProvider.LoadConfig<PlaytimeGift.GiftConfig>(ConfigProvider.ConfigType.PlaytimeGift);
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (_usePlaytimeGift && PlaytimeGiftConfig == null)
            {
                PlaytimeGiftConfig = ConfigProvider.LoadConfig<PlaytimeGift.GiftConfig>(ConfigProvider.ConfigType.PlaytimeGift);
            }
        }
#endif

        public void Init(IResourceGiver resourceGiver, string language = "en")
        {
            SGAManager.Init(this, RewardPreviewPopup, Notification);
            Localization.LocalizationManager.Init(LocalizationConfig, language);
            PlaytimeGift.GiftManager.Init(PlaytimeGiftConfig, resourceGiver);
        }
    }
}

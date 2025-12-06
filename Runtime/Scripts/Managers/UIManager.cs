namespace Serbull.GameAssets
{
    public class UIManager : IUIService
    {
        public readonly RewardPreviewPopup RewardPreviewPopup;
        public readonly Notification Notification;
        public readonly PlaytimeGift.GiftPopup PlaytimeGiftPopup;

        public UIManager(RewardPreviewPopup rewardPreviewPopup, Notification notification, PlaytimeGift.GiftPopup playtimeGiftPopup)
        {
            RewardPreviewPopup = rewardPreviewPopup;
            Notification = notification;
            PlaytimeGiftPopup = playtimeGiftPopup;
        }
    }
}

namespace Serbull.GameAssets
{
    public class UIManager : IUIService
    {
        public readonly RewardPreviewPopup RewardPreviewPopup;
        public readonly Notification Notification;
        public readonly PlaytimeGift.GiftPopup PlaytimeGiftPopup;
        public readonly Roulette.RoulettePopup RoulettePopup;
        public readonly Interact.InteractButton InteractButton;

        public UIManager(RewardPreviewPopup rewardPreviewPopup, Notification notification,
            PlaytimeGift.GiftPopup playtimeGiftPopup, Roulette.RoulettePopup roulettePopup,
            Interact.InteractButton interactButton)
        {
            RewardPreviewPopup = rewardPreviewPopup;
            Notification = notification;
            PlaytimeGiftPopup = playtimeGiftPopup;
            RoulettePopup = roulettePopup;
            InteractButton = interactButton;
        }
    }
}

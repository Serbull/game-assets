
namespace Serbull.GameAssets
{
    public class UIManager
    {
        public readonly RewardPreviewPopup RewardPreviewPopup;
        public readonly Notification Notification;
        public readonly PlaytimeReward.GiftPopup PlaytimeGiftPopup;
        public readonly DailyReward.RewardPopup DailyRewardPopup;
        public readonly Roulette.RoulettePopup RoulettePopup;
        public readonly Interact.InteractButton InteractButton;

        public UIManager(RewardPreviewPopup rewardPreviewPopup, Notification notification,
            PlaytimeReward.GiftPopup playtimeGiftPopup, DailyReward.RewardPopup dailyRewardPopup,
            Roulette.RoulettePopup roulettePopup, Interact.InteractButton interactButton)
        {
            RewardPreviewPopup = rewardPreviewPopup;
            Notification = notification;
            PlaytimeGiftPopup = playtimeGiftPopup;
            DailyRewardPopup = dailyRewardPopup;
            RoulettePopup = roulettePopup;
            InteractButton = interactButton;
        }
    }
}

namespace Serbull.GameAssets
{
    public interface IPlaytimeGiftService
    {
        int GetNearestGiftId();
        void ClaimReward(int id);
        PlaytimeGift.GiftConfig Config { get; }
    }
}

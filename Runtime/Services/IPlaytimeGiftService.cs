namespace Serbull.GameAssets
{
    public interface IPlaytimeGiftService
    {
        int GetNearestGiftId();
        void ClaimReward(int id);
        PlaytimeReward.GiftConfig Config { get; }
    }
}

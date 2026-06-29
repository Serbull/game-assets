using System;

namespace Serbull.GameAssets.Samples
{
    [Serializable]
    public class SaveData
    {
        public int money = 1000;
        public Roulette.SaveData Roulette = new();
        public DailyReward.SaveData DailyReward = new();
    }
}

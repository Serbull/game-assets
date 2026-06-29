using System;

namespace Serbull.GameAssets.Samples
{
    [Serializable]
    public class SaveData
    {
        public int Money = 1000;
        public Roulette.RouletteData Roulette = new();
        public DailyReward.SaveData DailyReward = new();
    }
}

using System;
using System.Collections.Generic;

namespace Serbull.GameAssets.DailyReward
{
    [Serializable]
    public class SaveData
    {
        public string lastLoginDate;
        public int totalDays;
        public List<int> claimedRewards = new();
    }
}

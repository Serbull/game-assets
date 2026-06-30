using System;
using UnityEngine;
using Serbull.GameAssets.Rarity;
using Serbull.GameAssets.Reward;

namespace Serbull.GameAssets.PlaytimeReward
{
    public class RewardConfig : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public RewardData reward;
            [RarityDropdown] public string color;
            public int time;
        }

        public Entry[] entries;

        public Entry GetEntry(int id) => entries[id];
    }
}

using System;
using UnityEngine;
using Serbull.GameAssets.Rarity;

namespace Serbull.GameAssets.PlaytimeReward
{
    public class RewardConfig : ScriptableObject
    {
        public enum ResourceType
        {
            Custom, Egg, Pet, LuckySpin
        }

        [Serializable]
        public class Reward
        {
            public ResourceType ResourceType = ResourceType.Custom;
            public string ResourceId;
            [RarityDropdown] public string Color;
            public Sprite Icon;
            public int Count;
            public int Time;
        }

        public Reward[] Datas;

        public Reward GetReward(int id)
        {
            return Datas[id];
        }
    }
}

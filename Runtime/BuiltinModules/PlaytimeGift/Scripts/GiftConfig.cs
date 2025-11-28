using System;
using UnityEngine;

namespace Serbull.GameAssets.PlaytimeGift
{
    public class GiftConfig : ScriptableObject
    {
        [Serializable]
        public class Reward
        {
            public string Resource;
            [Rare.RareDropdown] public string Color;
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

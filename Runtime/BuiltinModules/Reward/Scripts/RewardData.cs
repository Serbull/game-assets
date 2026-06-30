using UnityEngine;

namespace Serbull.GameAssets.Reward
{
    [System.Serializable]
    public class RewardData
    {
        public enum RewardType
        {
            Custom, Egg, Pet, LuckySpin
        }

        public RewardType type;
        public string id;
        public int count;
        public Sprite icon;
    }
}

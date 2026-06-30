using UnityEngine;

namespace Serbull.GameAssets.Reward
{
    [System.Serializable]
    public class RewardData
    {
        public enum RewardType
        {
            Custom, Egg, Pet, RouletteSpin
        }

        public RewardType type;
        public string id;
        public int count;
        public Sprite icon;


        public static RewardData Custom(string resourceId, int count, Sprite icon = null)
        {
            return new RewardData { type = RewardType.Custom, id = resourceId, count = count, icon = icon };
        }

        public static RewardData RouletteSpin(int count)
        {
            Sprite icon = Services.Roulette?.Config.SpinIcon;
            return new RewardData { type = RewardType.RouletteSpin, count = count, icon = icon };
        }
    }
}

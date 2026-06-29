using System;
using UnityEngine;
using Serbull.GameAssets.Rarity;

namespace Serbull.GameAssets.DailyReward
{
    public class RewardConfig : ScriptableObject
    {
        [Serializable]
        public class Reward
        {
            public int Day;
            public string Resource;
            public Sprite Icon;
            public int Count;
            public Sprite BgSprite;
            public bool UseRarityColor = true;
            [ShowIf(nameof(UseRarityColor)), RarityDropdown]
            public string BgColorStr;
            [ShowIf(nameof(UseRarityColor), true)]
            public Color BgColor = Color.white;
        }

        public Reward[] Datas;

        public Reward GetReward(int id)
        {
            return Datas[id];
        }

        [Button("7 days preset (7 items)", 10)]
        private void Create7DaysPreset()
        {
            CreatePreset(new[] { 1, 2, 3, 4, 5, 6, 7 });
        }

        [Button("14 days preset (14 items)")]
        private void Create14DaysPreset()
        {
            CreatePreset(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 });
        }

        [Button("30 days preset (14 items)")]
        private void Create30DaysPreset()
        {
            CreatePreset(new[] { 1, 2, 3, 4, 5, 6, 7, 9, 12, 15, 18, 21, 25, 30 });
        }

        private void CreatePreset(int[] days)
        {
            Array.Resize(ref Datas, days.Length);
            for (int i = 0; i < days.Length; i++)
            {
                if (Datas[i] == null)
                {
                    Datas[i] = new()
                    {
                        Count = 1,
                        BgColorStr = "rare"
                    };
                }
                Datas[i].Day = days[i];
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}

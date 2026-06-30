using System;
using UnityEngine;
using Serbull.GameAssets.Rarity;
using Serbull.GameAssets.Reward;

namespace Serbull.GameAssets.DailyReward
{
    public class RewardConfig : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public int day;
            public RewardData reward;
            public Sprite bgSprite;
            public bool bgRarityColor = true;
            [ShowIf(nameof(bgRarityColor)), RarityDropdown]
            public string bgColorStr;
            [ShowIf(nameof(bgRarityColor), true)]
            public Color bgColor = Color.white;
        }

        public Entry[] entries;

        public Entry GetEntry(int id) => entries[id];

#if UNITY_EDITOR
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
            Array.Resize(ref entries, days.Length);
            for (int i = 0; i < days.Length; i++)
            {
                if (entries[i] == null)
                {
                    entries[i] = new()
                    {
                        bgColorStr = "rare"
                    };
                }
                entries[i].day = days[i];
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}

using UnityEngine;
using System;

namespace Serbull.GameAssets.Roulette
{
    public class RouletteConfig : ScriptableObject
    {
        [Serializable]
        public class PartData
        {
            public int Weight;
            public RewardData Reward;
        }

        [Serializable]
        public class RewardData
        {
            public string Resource;
            public Sprite Icon;
            public int Count;
        }

        public PartData[] Parts;

        [Space]
        public bool UseInApps = true;
        [ShowIf(nameof(UseInApps))] public int InApp1AddSpins = 5;
        [ShowIf(nameof(UseInApps))] public int InApp2AddSpins = 20;
        [Space]
        public bool AddFreeSpins;
        [ShowIf(nameof(AddFreeSpins))] public int FreeSpinTimer = 300;

        public int[] GetWeightIndexes()
        {
            var result = new int[Parts.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Parts[i].Weight;
            }

            return result;
        }
    }
}

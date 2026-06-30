using UnityEngine;
using System;
using Serbull.GameAssets.Reward;

namespace Serbull.GameAssets.Roulette
{
    public class RouletteConfig : ScriptableObject
    {
        [Serializable]
        public class PartData
        {
            public int weight;
            public RewardData reward;
        }

        public PartData[] Parts;

        [Space]
        public bool UseInApps = true;
        [ShowIf(nameof(UseInApps))] public int InApp1AddSpins = 5;
        [ShowIf(nameof(UseInApps))] public int InApp2AddSpins = 20;
        [ShowIf(nameof(UseInApps))] public Sprite InAppSprite;
        [Space]
        public bool AddFreeSpins;
        [ShowIf(nameof(AddFreeSpins))] public int FreeSpinTimer = 900;

        public int[] GetWeightIndexes()
        {
            var result = new int[Parts.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Parts[i].weight;
            }

            return result;
        }
    }
}

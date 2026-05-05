using System;
using Serbull.GameAssets.Localization;
using UnityEngine;

namespace Serbull.GameAssets.Rarity
{
    public class RarityConfig : ScriptableObject
    {
        [Serializable]
        public class RarityData
        {
            public string Id;
            public string LocalizationId;
            public Color Color;
        }

        public RarityData[] Rarities;

        public LocalizationData[] Localizations;

        public RarityData GetRareData(string id)
        {
            foreach (var data in Rarities)
            {
                if (data.Id == id)
                {
                    return data;
                }
            }

            Debug.LogError($"Not exist rare '{id}'");
            return Rarities[0];
        }
    }
}

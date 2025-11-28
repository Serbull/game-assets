using System;
using UnityEngine;

namespace Serbull.GameAssets.Rare
{
    public class RareConfig : ScriptableObject
    {
        [Serializable]
        public class RareData
        {
            public string Id;
            public Color Color;
        }

        public RareData[] Rares;

        public RareData GetRareData(string id)
        {
            foreach (var data in Rares)
            {
                if (data.Id == id)
                {
                    return data;
                }
            }

            Debug.LogError($"Not exist rare '{id}'");
            return Rares[0];
        }
    }
}

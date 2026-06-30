using UnityEngine;
using Serbull.GameAssets.Rarity;

namespace Serbull.GameAssets
{
    public class RarityService
    {
        private readonly RarityConfig _config;

        public RarityConfig Config => _config;

        public RarityService(RarityConfig config)
        {
            _config = config;
        }

        public RarityData GetRarityData(string id)
        {
            foreach (var data in _config.Rarities)
            {
                if (data.Id == id)
                {
                    return data;
                }
            }

            Debug.LogError($"Not exist rare '{id}'");
            return _config.Rarities[0];
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using Serbull.GameAssets.PlaytimeReward;

namespace Serbull.GameAssets
{
    public class PlaytimeRewardService
    {
        private static readonly List<int> _claimedList = new();

        private readonly RewardConfig _config;
        private readonly IResourceGiver _resourceGiver;
        public RewardConfig Config => _config;

        public PlaytimeRewardService(RewardConfig config, IResourceGiver resourceGiver)
        {
            _config = config;
            _resourceGiver = resourceGiver;
        }

        public bool RewardIsClaimed(int id) => _claimedList.Contains(id);

        public int GetNearestGiftId()
        {
            for (int i = 0; i < _config.entries.Length; i++)
            {
                if (_claimedList.Contains(i))
                {
                    continue;
                }

                return i;
            }

            return -1;
        }

        public void ClaimReward(int id)
        {
            _claimedList.Add(id);
            var data = _config.GetEntry(id).reward;
            Services.Reward.AddReward(data, true);
        }
    }
}

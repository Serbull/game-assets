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
            for (int i = 0; i < _config.Datas.Length; i++)
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
            var data = _config.GetReward(id);

            if ((data.ResourceType == RewardConfig.ResourceType.Egg
                || data.ResourceType == RewardConfig.ResourceType.Pet)
                && Services.PetService == null)
            {
                Debug.LogError("Pet Service is null. Add PetService in Services.");
                return;
            }

            _claimedList.Add(id);

            switch (data.ResourceType)
            {
                case RewardConfig.ResourceType.Egg:
                    Services.PetService.AddEggWithPreview(data.ResourceId);
                    break;
                case RewardConfig.ResourceType.Pet:
                    Services.PetService.AddPetWithPreview(data.ResourceId);
                    break;
                case RewardConfig.ResourceType.LuckySpin:
                    Services.Roulette.AddSpinWithPreview(data.Count);
                    break;
                case RewardConfig.ResourceType.Custom:
                    _resourceGiver.AddResource(data.ResourceId, data.Count);
                    var item = new RewardPreviewItem(data.Icon, data.Count);
                    Services.UI.RewardPreviewPopup.Show(item);
                    break;
                default:
                    Debug.LogError("Not exist: " + data.ResourceType);
                    break;
            }
        }
    }
}

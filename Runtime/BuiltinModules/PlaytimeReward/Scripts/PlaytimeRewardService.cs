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
            var data = _config.GetEntry(id).reward;

            if ((data.type == RewardData.RewardType.Egg
                || data.type == RewardData.RewardType.Pet)
                && Services.PetService == null)
            {
                Debug.LogError("Pet Service is null. Add PetService in Services.");
                return;
            }

            _claimedList.Add(id);

            switch (data.type)
            {
                case RewardData.RewardType.Egg:
                    Services.PetService.AddEggWithPreview(data.id);
                    break;
                case RewardData.RewardType.Pet:
                    Services.PetService.AddPetWithPreview(data.id);
                    break;
                case RewardData.RewardType.LuckySpin:
                    Services.Roulette.AddSpinWithPreview(data.count);
                    break;
                case RewardData.RewardType.Custom:
                    _resourceGiver.AddResource(data.id, data.count);
                    var item = new RewardPreviewItem(data.icon, data.count);
                    Services.UI.RewardPreviewPopup.Show(item);
                    break;
                default:
                    Debug.LogError("Not exist: " + data.id);
                    break;
            }
        }
    }
}

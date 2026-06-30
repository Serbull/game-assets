using UnityEngine;
using Serbull.GameAssets.Reward;

namespace Serbull.GameAssets
{
    public class RewardService
    {
        private readonly IResourceGiver _resourceGiver;
        private readonly RewardPreviewPopup _rewardPreviewPopup;

        public RewardService(IResourceGiver resourceGiver, RewardPreviewPopup rewardPreviewPopup)
        {
            _resourceGiver = resourceGiver;
            _rewardPreviewPopup = rewardPreviewPopup;
        }

        public void AddReward(RewardData reward, bool showPreview)
        {
            if ((reward.type == RewardData.RewardType.Egg
                || reward.type == RewardData.RewardType.Pet)
                && Services.PetService == null)
            {
                Debug.LogError("Pet Service is null. Add PetService in Services.");
                return;
            }

            switch (reward.type)
            {
                case RewardData.RewardType.Egg:
                    Services.PetService.AddEggWithPreview(reward.id);
                    break;
                case RewardData.RewardType.Pet:
                    Services.PetService.AddPet(reward.id);
                    if (showPreview) Preview(reward);
                    break;
                case RewardData.RewardType.LuckySpin:
                    Services.Roulette.AddSpin(reward.count);
                    if (showPreview) Preview(reward);
                    break;
                case RewardData.RewardType.Custom:
                    _resourceGiver.AddResource(reward.id, reward.count);
                    if (showPreview) Preview(reward);
                    break;
                default:
                    Debug.LogError("Not exist: " + reward.id);
                    break;
            }
        }

        public void Preview(RewardData reward)
        {
            if (_rewardPreviewPopup == null)
            {
                Debug.LogError("PreviewPopup is null. Add it in SGAInstaller.");
                return;
            }

            var luckySpinReward = new RewardPreviewItem("", "", reward.icon, reward.count, true, Color.white, Color.white, Color.white);
            _rewardPreviewPopup.Show(luckySpinReward);
        }
    }
}

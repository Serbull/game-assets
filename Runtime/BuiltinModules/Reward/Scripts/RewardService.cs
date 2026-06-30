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
            AddReward(reward, showPreview, null);
        }

        public void AddReward(RewardData reward, params RewardPreviewItem[] customPreviewItems)
        {
            AddReward(reward, true, customPreviewItems);
        }

        public void AddReward(RewardData reward, bool showPreview, params RewardPreviewItem[] customPreviewItems)
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
                    if (showPreview) Preview(reward, customPreviewItems);
                    break;
                default:
                    Debug.LogError("Not exist: " + reward.id);
                    break;
            }
        }

        public void Preview(RewardData reward)
        {
            Preview(reward, null);
        }

        public void Preview(RewardData reward, params RewardPreviewItem[] customPreviewItems)
        {
            if (_rewardPreviewPopup == null)
            {
                Debug.LogError("PreviewPopup is null. Add it in SGAInstaller.");
                return;
            }

            if (customPreviewItems != null)
            {
                _rewardPreviewPopup.Show(customPreviewItems);
                return;
            }

            var item = new RewardPreviewItem("", "", reward.icon, reward.count, true, Color.white, Color.white, Color.white);
            _rewardPreviewPopup.Show(item);
        }

        public void PreviewPet(string petId)
        {
            if (_rewardPreviewPopup == null)
            {
                Debug.LogError("PreviewPopup is null. Add it in SGAInstaller.");
                return;
            }

            if (Services.PetService == null)
            {
                Debug.LogError("PetService is null. Add PetInstaller on Scene.");
                return;
            }

            var petData = Services.PetService.GetPetData(petId);
            var rarityData = Services.Rarity.GetRarityData(petData.rarity);

            var item = new RewardPreviewItem(Services.Localization.GetText(petData.title),
                Services.Localization.GetText(rarityData.LocalizationId),
                petData.icon, 1, true,
                Color.white, rarityData.Color, rarityData.Color);

            Services.UI.RewardPreviewPopup.Show(item);
        }
    }
}

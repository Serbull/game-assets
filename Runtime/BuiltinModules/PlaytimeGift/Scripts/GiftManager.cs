using System.Collections.Generic;
using UnityEngine;

namespace Serbull.GameAssets.PlaytimeGift
{
    public class GiftManager : IPlaytimeGiftService
    {
        private readonly GiftConfig _giftConfig;
        private readonly IResourceGiver _resourceGiver;

        public static readonly List<int> ClaimedList = new();

        public GiftConfig Config => _giftConfig;

        public GiftManager(GiftConfig giftConfig, IResourceGiver resourceGiver)
        {
            _giftConfig = giftConfig;
            _resourceGiver = resourceGiver;
        }

        public int GetNearestGiftId()
        {
            for (int i = 0; i < _giftConfig.Datas.Length; i++)
            {
                if (ClaimedList.Contains(i))
                {
                    continue;
                }

                return i;
            }

            return -1;
        }

        public void ClaimReward(int id)
        {
            ClaimedList.Add(id);
            var data = _giftConfig.GetReward(id);
            var color = Services.Rarity.Config.GetRarityData(data.Color).Color;
            var item = new RewardPreviewItem("", "", data.Icon, data.Count, true, Color.white, Color.white, color);
            _resourceGiver.AddResource(data.Resource, data.Count);
            Services.UI.RewardPreviewPopup.Show(item);
        }
    }
}

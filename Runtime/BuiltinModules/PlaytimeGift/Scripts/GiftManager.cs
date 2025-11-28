using System.Collections.Generic;
using UnityEngine;

namespace Serbull.GameAssets.PlaytimeGift
{
    public class GiftManager
    {
        private static GiftConfig _giftConfig;
        private static IResourceGiver _resourceGiver;

        public static readonly List<int> ClaimedList = new();

        public static void Init(GiftConfig giftConfig, IResourceGiver resourceGiver)
        {
            _giftConfig = giftConfig;
            _resourceGiver = resourceGiver;
        }

        public static int GetNearestGift()
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

        public static void ClaimReward(int id)
        {
            ClaimedList.Add(id);
            var data = _giftConfig.GetReward(id);
            var item = new RewardPreviewItem("", "", data.Icon, data.Count, true, Color.white, Color.white, Color.white);
            SGAManager.RewardPreviewPopup.Show(item);
            _resourceGiver.AddResource(data.Resource, data.Count);
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Serbull.GameAssets.DailyReward
{
    public class RewardPopup : Popup
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private RewardSlot[] _rewardSlots;

        private void Awake()
        {
            _closeButton.onClick.AddListener(Hide);
        }

        private void OnEnable()
        {
            if (Services.DailyReward != null)
                Services.DailyReward.OnUpdated += UpdateUI;

            UpdateUI();
        }

        private void OnDisable()
        {
            if (Services.DailyReward != null)
                Services.DailyReward.OnUpdated -= UpdateUI;
        }

        public void UpdateUI()
        {
            if (Services.DailyReward == null) return;

            var config = Services.DailyReward.RewardConfig;

            var count = Mathf.Min(config.Datas.Length, _rewardSlots.Length);

            if (count < _rewardSlots.Length)
            {
                Debug.LogError("Config contains less elements than in popup");
            }

            for (int i = 0; i < count; i++)
            {
                var id = i;
                var data = config.Datas[i];
                var bgColor = data.UseRarityColor ?
                    Services.Rarity.GetRarityData(data.BgColorStr).Color : data.BgColor;
                var available = Services.DailyReward.IsRewardAvailable(i);
                var claimed = Services.DailyReward.IsRewardClaimed(i);

                _rewardSlots[i].Init(data.BgSprite, bgColor, data.Day,
                    data.Icon, data.Count,
                    available, claimed, () => OnItemClicked(id));
            }
        }

        private void OnItemClicked(int id)
        {
            if (!Services.DailyReward.IsRewardAvailable(id))
                return;

            if (Services.DailyReward.IsRewardClaimed(id))
                return;

            Services.DailyReward.ClaimReward(id);
        }
    }
}

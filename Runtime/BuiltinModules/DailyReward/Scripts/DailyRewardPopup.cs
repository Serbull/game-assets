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

            var count = Mathf.Min(config.entries.Length, _rewardSlots.Length);

            if (count < _rewardSlots.Length)
            {
                Debug.LogError("Config contains less elements than in popup");
            }

            for (int i = 0; i < count; i++)
            {
                var id = i;
                var data = config.entries[i];
                var bgColor = data.bgRarityColor ?
                    Services.Rarity.GetRarityData(data.bgColorStr).Color : data.bgColor;
                var available = Services.DailyReward.IsRewardAvailable(i);
                var claimed = Services.DailyReward.IsRewardClaimed(i);
                var reward = data.reward;

                _rewardSlots[i].Init(data.bgSprite, bgColor, data.day,
                    reward.icon, reward.count,
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

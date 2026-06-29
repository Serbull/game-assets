using System;
using Serbull.GameAssets.DailyReward;
using UnityEngine;

namespace Serbull.GameAssets
{
    public class DailyRewardService
    {
        private const string DateFormat = "yyyy-MM-dd";
        public event Action OnUpdated;

        private readonly RewardConfig _config;
        private readonly SaveData _saveData;

        public RewardConfig RewardConfig => _config;

        public DailyRewardService(RewardConfig config, SaveData saveData)
        {
            _config = config;
            _saveData = saveData;

            if (_config == null)
            {
                Debug.LogError("DailyReward Config is null");
                return;
            }

            if (_saveData == null)
            {
                Debug.LogWarning("DailyReward SaveData is null");
                return;
            }

            var today = DateTime.Now.Date.ToString(DateFormat);
            if (string.IsNullOrEmpty(_saveData.lastLoginDate) || today != _saveData.lastLoginDate)
            {
                _saveData.lastLoginDate = today;
                _saveData.totalDays++;
            }
        }

        public bool HasRewardToClaim()
        {
            for (int i = 0; i < _config.Datas.Length; i++)
            {
                if (_config.Datas[i].Day <= _saveData.totalDays
                    && !_saveData.claimedRewards.Contains(i))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsRewardAvailable(int index)
        {
            if (_config.Datas.Length <= index)
                return false;

            return _config.Datas[index].Day <= _saveData.totalDays;
        }

        public bool IsRewardClaimed(int index)
        {
            return _saveData.claimedRewards.Contains(index);
        }

        public void ClaimReward(int index)
        {
            if (_saveData.claimedRewards.Contains(index))
            {
                Debug.LogError($"Reward {index} already claimed");
                return;
            }

            _saveData.claimedRewards.Add(index);

            var data = _config.GetReward(index);

            Services.ResourceGiver.AddResource(data.Resource, data.Count);

            if (Services.UI.RewardPreviewPopup != null)
            {
                var preview = new RewardPreviewItem("", "", data.Icon, data.Count, true, Color.white, Color.white, Color.white);
                Services.UI.RewardPreviewPopup.Show(preview);
            }

            OnUpdated?.Invoke();
        }
    }
}

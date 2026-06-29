using System;
using UnityEngine;

namespace Serbull.GameAssets.Roulette
{
    public class RouletteService
    {
        public event Action OnSpinCountChanged;
        private static float _freeSpinTimer = -1;

        private readonly RouletteConfig _config;
        private readonly IResourceGiver _resourceGiver;
        private readonly SaveData _saveData;

        public RouletteService(RouletteConfig config, IResourceGiver resourceGiver, SaveData saveData)
        {
            _config = config;
            _resourceGiver = resourceGiver;
            _saveData = saveData;

            if (_freeSpinTimer == -1)
            {
                _freeSpinTimer = _config.FreeSpinTimer;
            }
        }

        public RouletteConfig Config => _config;

        public float FreeSpinTimer => _freeSpinTimer;

        public int SpinCount => _saveData.spinCount;

        public void Tick()
        {
            if (_config.AddFreeSpins)
            {
                _freeSpinTimer -= Time.deltaTime;
                if (_freeSpinTimer <= 0)
                {
                    AddSpin(1);
                    _freeSpinTimer += _config.FreeSpinTimer;
                }
            }
        }

        public void AddSpin(int amount)
        {
            _saveData.spinCount += amount;
            OnSpinCountChanged?.Invoke();
        }

        public void SpendSpin(int amount)
        {
            _saveData.spinCount -= amount;
            OnSpinCountChanged?.Invoke();
        }

        public void AddSpinWithPreview(int amount)
        {
            AddSpin(amount);
            var icon = Services.Roulette.Config.InAppSprite;
            var luckySpinReward = new RewardPreviewItem("", "", icon, amount, true, Color.white, Color.white, Color.white);
            Services.UI.RewardPreviewPopup.Show(luckySpinReward);
        }

        public void AddRewardWithPreview(RouletteConfig.RewardData reward)
        {
            _resourceGiver.AddResource(reward.Resource, reward.Count);
            var item = new RewardPreviewItem("", "", reward.Icon, reward.Count, true, Color.white, Color.white, Color.white);
            Services.UI.RewardPreviewPopup.Show(item);
        }
    }
}

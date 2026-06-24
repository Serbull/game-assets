using System;
using UnityEngine;

namespace Serbull.GameAssets.Roulette
{
    public class RouletteService
    {
        public event Action<int> OnSpinCountChanged;
        private static float _freeSpinTimer = -1;

        private readonly RouletteConfig _config;
        private readonly IResourceService _resourceService;
        private readonly RouletteData _rouletteData;

        public RouletteService(RouletteConfig config, IResourceService resourceService, RouletteData rouletteData)
        {
            _config = config;
            _resourceService = resourceService;
            _rouletteData = rouletteData;

            if (_freeSpinTimer == -1)
            {
                _freeSpinTimer = _config.FreeSpinTimer;
            }
        }

        public RouletteConfig Config => _config;

        public float FreeSpinTimer => _freeSpinTimer;

        public RouletteData SaveData => _rouletteData;

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
            _rouletteData.SpinCount += amount;
            OnSpinCountChanged?.Invoke(_rouletteData.SpinCount);
        }

        public void SpendSpin(int amount)
        {
            _rouletteData.SpinCount -= amount;
            OnSpinCountChanged?.Invoke(_rouletteData.SpinCount);
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
            _resourceService.AddResource(reward.Resource, reward.Count);
            var item = new RewardPreviewItem("", "", reward.Icon, reward.Count, true, Color.white, Color.white, Color.white);
            Services.UI.RewardPreviewPopup.Show(item);
        }
    }
}

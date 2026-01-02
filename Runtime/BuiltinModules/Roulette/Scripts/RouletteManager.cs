using UnityEngine;

namespace Serbull.GameAssets.Roulette
{
    public struct ChangeLuckySpinEvent
    {
        public long Amount;

        public ChangeLuckySpinEvent(long amount)
        {
            Amount = amount;
        }
    }

    public class RouletteManager : IRouletteService
    {
        private static float _freeSpinTimer = -1;

        private readonly RouletteConfig _config;
        private readonly IResourceGiver _resourceGiver;
        private readonly RouletteData _rouletteData;

        public RouletteManager(RouletteConfig config, IResourceGiver resourceGiver, RouletteData rouletteData)
        {
            _config = config;
            _resourceGiver = resourceGiver;
            _rouletteData = rouletteData;

            if (_freeSpinTimer == -1)
            {
                _freeSpinTimer = _config.FreeSpinTimer;
            }

            EventBus.Publish(new ChangeLuckySpinEvent(rouletteData.SpinCount));
        }

        public RouletteConfig Config => _config;

        public float FreeSpinTimer => _freeSpinTimer;

        public RouletteData SaveData => _rouletteData;

        public void AddReward(RouletteConfig.RewardData reward)
        {
            var item = new RewardPreviewItem("", "", reward.Icon, reward.Count, true, Color.white, Color.white, Color.white);
            _resourceGiver.AddResource(reward.Resource, reward.Count);
            Services.UI.RewardPreviewPopup.Show(item);
        }

        public void Update()
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
            EventBus.Publish(new ChangeLuckySpinEvent(_rouletteData.SpinCount));
        }

        public void SpendSpin(int amount)
        {
            _rouletteData.SpinCount -= amount;
            EventBus.Publish(new ChangeLuckySpinEvent(_rouletteData.SpinCount));
        }
    }
}

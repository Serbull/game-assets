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
        private readonly ICurrency _luckySpin;

        public RouletteManager(RouletteConfig config, IResourceGiver resourceGiver, ICurrency luckySpin)
        {
            _config = config;
            _resourceGiver = resourceGiver;
            _luckySpin = luckySpin;

            if (_freeSpinTimer == -1)
            {
                _freeSpinTimer = _config.FreeSpinTimer;
            }

            EventBus.Publish(new ChangeLuckySpinEvent(luckySpin.Amount));
        }

        public RouletteConfig Config => _config;

        public ICurrency LuckySpin => _luckySpin;

        public float FreeSpinTimer => _freeSpinTimer;

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
                    AddLuckySpin(1);
                    _freeSpinTimer += _config.FreeSpinTimer;
                }
            }
        }

        public void AddLuckySpin(int amount)
        {
            _luckySpin.Add(amount);
            EventBus.Publish(new ChangeLuckySpinEvent(_luckySpin.Amount));
        }

        public void SpendLuckySpin(int amount)
        {
            _luckySpin.Spend(amount);
            EventBus.Publish(new ChangeLuckySpinEvent(_luckySpin.Amount));
        }
    }
}

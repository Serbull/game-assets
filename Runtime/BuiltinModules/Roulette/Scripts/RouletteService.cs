using System;
using UnityEngine;
using Serbull.GameAssets.Roulette;

namespace Serbull.GameAssets
{
    public class RouletteService
    {
        public event Action OnSpinCountChanged;
        private static float _freeSpinTimer = -1;

        private readonly RouletteConfig _config;
        private readonly SaveData _saveData;

        public RouletteService(RouletteConfig config, SaveData saveData)
        {
            _config = config;
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
    }
}

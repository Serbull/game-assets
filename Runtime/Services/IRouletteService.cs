namespace Serbull.GameAssets
{
    public interface IRouletteService
    {
        public Roulette.RouletteConfig Config { get; }
        public Roulette.RouletteData SaveData { get; }
        public void AddReward(Roulette.RouletteConfig.RewardData reward);
        public float FreeSpinTimer { get; }
        public void Update();
        void AddSpin(int amount);
        void SpendSpin(int amount);
    }
}

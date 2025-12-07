namespace Serbull.GameAssets
{
    public interface IRouletteService
    {
        public Roulette.RouletteConfig Config { get; }
        public ICurrency LuckySpin { get; }
        public void AddReward(Roulette.RouletteConfig.RewardData reward);
        public float FreeSpinTimer { get; }
        public void Update();
        void AddLuckySpin(int amount);
        void SpendLuckySpin(int amount);
    }
}

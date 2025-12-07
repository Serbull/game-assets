
namespace Serbull.GameAssets.Samples
{
    public class LuckySpin : ICurrency
    {
        private readonly SaveData _saveData;

        public LuckySpin(SaveData saveData)
        {
            _saveData = saveData;
        }

        public long Amount => _saveData.LuckySpins;

        public void Add(long amount)
        {
            _saveData.LuckySpins += (int)amount;
        }

        public void Spend(long amount)
        {
            _saveData.LuckySpins -= (int)amount;
        }
    }
}

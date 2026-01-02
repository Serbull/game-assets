using System;

namespace Serbull.GameAssets.Samples
{
    [Serializable]
    public class SaveData
    {
        public int Money = 1000;
        public Roulette.RouletteData RouletteData = new();
    }
}

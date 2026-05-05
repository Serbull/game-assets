namespace Serbull.GameAssets.Rarity
{
    public class RarityService
    {
        private readonly RarityConfig _config;

        public RarityConfig Config => _config;

        public RarityService(RarityConfig config)
        {
            _config = config;
        }
    }
}

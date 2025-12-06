namespace Serbull.GameAssets.Rare
{
    public class RareManager: IRareService
    {
        private readonly RareConfig _config;

        public RareConfig Config => _config;

        public RareManager(RareConfig config)
        {
            _config = config;
        }
    }
}

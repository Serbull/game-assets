using System;

namespace Serbull.GameAssets.Samples
{
    public class PurchaseService : IPurchaseService
    {
        public event Action<string> OnRewardGranted;
        public event Action OnAnyRewardGranted;
    }
}

using System;

namespace Serbull.GameAssets
{
    public interface IPurchaseService
    {
        event Action<string> OnRewardGranted;
        event Action OnAnyRewardGranted;
    }
}

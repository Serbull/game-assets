using System;
using System.Collections.Generic;

namespace Serbull.GameAssets.Localization
{
    [Serializable]
    public class LocalizationData : IEqualityComparer<LocalizationData>
    {
        public string Id;
        public string En;
        public string Ru;

        public bool Equals(LocalizationData x, LocalizationData y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(LocalizationData obj)
        {
            throw new NotImplementedException();
        }
    }
}

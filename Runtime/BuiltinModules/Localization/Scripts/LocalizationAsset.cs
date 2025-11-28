using UnityEngine;

namespace Serbull.GameAssets.Localization
{
    public class LocalizationAsset : MonoBehaviour
    {
        public LocalizationData[] Localizations;

        private void Awake()
        {
            LocalizationManager.AddLocalization(Localizations);
        }
    }
}

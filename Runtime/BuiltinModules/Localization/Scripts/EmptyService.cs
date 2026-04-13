using System.Collections.Generic;

namespace Serbull.GameAssets.Localization
{
    public class EmptyService : ILocalizationService
    {
        public HashSet<LocalizationData> Localizations = new();

        public void AddLocalization(params LocalizationData[] localizationData)
        {
            foreach (var loc in localizationData)
            {
                Localizations.Add(loc);
            }
        }

        public string GetText(string id)
        {
            return id;
        }
    }
}

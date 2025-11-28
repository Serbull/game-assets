using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Serbull.GameAssets.Localization
{
    public class LocalizationManager
    {
        private static HashSet<LocalizationData> _localizations = new();
        private static string _language;

        public static void Init(LocalizationConfig config, string langugage)
        {
            AddLocalization(config.Localizations);
            _language = langugage;
        }

        public static string GetText(string id)
        {
            foreach (var loc in _localizations)
            {
                if (loc.Id == id)
                {
                    if (_language == "ru")
                        return loc.Ru;
                    else
                        return loc.En;
                }
            }

            Debug.LogError($"Not exist localization with id '{id}'");
            return id;
        }

        public static void AddLocalization(params LocalizationData[] localizationData)
        {
            foreach (var loc in localizationData)
            {
                if (!_localizations.Contains(loc, loc))
                {
                    _localizations.Add(loc);
                }
            }
        }
    }
}

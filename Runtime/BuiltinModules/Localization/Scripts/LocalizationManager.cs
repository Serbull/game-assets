using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Serbull.GameAssets.Localization
{
    public struct UpdateLocalizationEvent { }

    public class LocalizationManager : ILocalizationService
    {
        private readonly HashSet<LocalizationData> _localizations = new();
        private readonly string _language;

        public LocalizationManager(LocalizationConfig config, string langugage)
        {
            AddLocalization(config.Localizations);
            _language = langugage;
            EventBus.Publish(new UpdateLocalizationEvent());
        }

        public string GetText(string id)
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

        public void AddLocalization(params LocalizationData[] localizationData)
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

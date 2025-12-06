using Serbull.GameAssets.Localization;

namespace Serbull.GameAssets
{
    public interface ILocalizationService
    {
        string GetText(string id);
        void AddLocalization(params LocalizationData[] localizationData);
    }
}

using System.Globalization;
using MarBasSchema.Sys;

namespace MarBasSchema.Broker
{
    public interface ISystemLanguageBroker
    {
        ISystemLanguage? GetSystemLanguage(CultureInfo culture);
        ISystemLanguage? CreateSystemLanguage(CultureInfo culture);
        int DeleteSystemLanguages(IEnumerable<ISystemLanguageRef> languages);
        int StoreSystemLanguages(IEnumerable<ISystemLanguage> languages);
        IEnumerable<ISystemLanguage> ListSystemLanguages(IEnumerable<CultureInfo>? cultures = null);
        IEnumerable<bool> CheckSystemLanguagesExist(IEnumerable<string> languages);
    }
}

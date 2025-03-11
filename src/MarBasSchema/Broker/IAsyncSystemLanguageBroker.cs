using System.Globalization;
using CraftedSolutions.MarBasSchema.Sys;

namespace CraftedSolutions.MarBasSchema.Broker
{
    public interface IAsyncSystemLanguageBroker
    {
        Task<ISystemLanguage?> GetSystemLanguageAsync(CultureInfo culture, CancellationToken cancellationToken = default);
        Task<ISystemLanguage?> CreateSystemLanguageAsync(CultureInfo culture, CancellationToken cancellationToken = default);
        Task<int> DeleteSystemLanguagesAsync(IEnumerable<ISystemLanguageRef> languages, CancellationToken cancellationToken = default);
        Task<int> StoreSystemLanguagesAsync(IEnumerable<ISystemLanguage> languages, CancellationToken cancellationToken = default);
        Task<IEnumerable<ISystemLanguage>> ListSystemLanguagesAsync(IEnumerable<CultureInfo>? cultures = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<bool>> CheckSystemLanguagesExistAsync(IEnumerable<string> languages, CancellationToken cancellationToken = default);
    }
}

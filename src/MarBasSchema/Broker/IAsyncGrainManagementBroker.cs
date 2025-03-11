using System.Globalization;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasSchema.Broker
{
    public interface IAsyncGrainManagementBroker
    {
        Task<IGrainLocalized?> GetGrainAsync(Guid id, CultureInfo? culture = null, CancellationToken cancellationToken = default);
        Task<IGrainBase?> CreateGrainAsync(string name, IIdentifiable parent, IIdentifiable? typedef, CancellationToken cancellationToken = default);
        Task<int> DeleteGrainsAsync(IEnumerable<IIdentifiable> ids, CancellationToken cancellationToken = default);
        Task<int> StoreGrainsAsync(IEnumerable<IGrainBase> grains, CancellationToken cancellationToken = default);
        Task<IGrainBase?> MoveGrainAsync(IIdentifiable grain, IIdentifiable newParent, CancellationToken cancellationToken = default);
        Task<bool> IsGrainInstanceOfAsync(IIdentifiable grain, IIdentifiable typedef, CancellationToken cancellationToken = default);
        Task<Type?> GetGrainTierAsync(IIdentifiable grain, CancellationToken cancellationToken = default);
        Task<IEnumerable<IGrainLocalized>> ListGrainsAsync(IIdentifiable? container, bool recursive = false, CultureInfo? culture = null, IEnumerable<IListSortOption<GrainSortField>>? sortOptions = null, IGrainQueryFilter? filter = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<IGrainLocalized>> ResolvePathAsync(string? path, CultureInfo? culture = null, IEnumerable<IListSortOption<GrainSortField>>? sortOptions = null, IGrainQueryFilter? filter = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<IGrainLocalized>> GetGrainAncestorsAsync(IIdentifiable grain, CultureInfo? culture = null, bool includeSelf = false, CancellationToken cancellationToken = default);
        Task<IDictionary<Guid, bool>> VerifyGrainsExistAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    }
}

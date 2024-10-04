using System.Globalization;
using MarBasCommon;
using MarBasSchema.Grain;

namespace MarBasSchema.Broker
{
    public interface IGrainManagementBroker
    {
        IGrainLocalized? GetGrain(Guid id, CultureInfo? culture = null);
        IGrainBase? CreateGrain(string name, IIdentifiable parent, IIdentifiable? typedef);
        int DeleteGrains(IEnumerable<IIdentifiable> ids);
        int StoreGrains(IEnumerable<IGrainBase> grains);
        IGrainBase? MoveGrain(IIdentifiable grain, IIdentifiable newParent);
        bool IsGrainInstanceOf(IIdentifiable grain, IIdentifiable typedef);
        Type? GetGrainTier(IIdentifiable grain);
        IEnumerable<IGrainLocalized> ListGrains(IIdentifiable? container, bool recursive = false, CultureInfo? culture = null, IEnumerable<IListSortOption<GrainSortField>>? sortOptions = null, IGrainQueryFilter? filter = null);
        IEnumerable<IGrainLocalized> ResolvePath(string? path, CultureInfo? culture = null, IEnumerable<IListSortOption<GrainSortField>>? sortOptions = null, IGrainQueryFilter? filter = null);
        IEnumerable<IGrainLocalized> GetGrainAncestors(IIdentifiable grain, CultureInfo? culture = null, bool includeSelf = false);
        IDictionary<Guid, bool> VerifyGrainsExist(IEnumerable<Guid> ids);
    }
}

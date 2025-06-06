﻿using System.Globalization;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasSchema.Broker
{
    public interface IAsyncTraitManagementBroker
    {
        Task<GrainTraitsMap> GetGrainTraitsAsync(IIdentifiable grain, CultureInfo? culture = null, CancellationToken cancellationToken = default);
        Task<ITraitBase?> GetTraitAsync(Guid id, CancellationToken cancellationToken = default);
        Task<int> DeleteTraitsAsync(IEnumerable<IIdentifiable> ids, CancellationToken cancellationToken = default);
        Task<int> StoreTraitsAsync(IEnumerable<ITraitBase> traits, CancellationToken cancellationToken = default);
        Task<ITraitBase?> CreateTraitAsync(ITraitRef traitRef, object? value = null, int ord = 0, CancellationToken cancellationToken = default);
        Task<IEnumerable<ITraitBase>> GetTraitValuesAsync(ITraitRef traitRef, CancellationToken cancellationToken = default);
        Task<int> ReplaceTraitValuesAsync<T>(ITraitRef traitRef, IEnumerable<T> values, CancellationToken cancellationToken = default);
        Task<int> ResetTraitValuesAsync(ITraitRef traitRef, CancellationToken cancellationToken = default);
        Task<int> ReindexTraitsAsync(IIdentifiable grain, IIdentifiable? propDef = null, CultureInfo? culture = null, int revision = -1, bool trimOverflow = false, CancellationToken cancellationToken = default);
        Task<IEnumerable<IGrainLocalized>> LookupGrainsByTraitAsync(ITraitRef traitRef, object? value = null, IEnumerable<IListSortOption<GrainSortField>>? sortOptions = null, CancellationToken cancellationToken = default);
    }
}

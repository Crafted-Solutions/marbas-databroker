using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;
using CraftedSolutions.MarBasSchema.GrainDef;
using System.Globalization;

namespace CraftedSolutions.MarBasSchema.Broker
{
    public interface IAsyncGrainDefManagementBroker
    {
        Task<IGrainTypeDefLocalized?> GetTypeDefAsync(Guid id, CultureInfo? culture = null, CancellationToken cancellationToken = default);
        Task<IGrainTypeDef?> CreateTypeDefAsync(string name, IIdentifiable? parent, string? implKey = null, IEnumerable<IIdentifiable>? mixins = null, CancellationToken cancellationToken = default);
        Task<int> StoreTypeDefsAsync(IEnumerable<IGrainTypeDef> typedefs, CancellationToken cancellationToken = default);
        Task<Type?> GetTypeDefTierAsync(IIdentifiable? typeDef, CancellationToken cancellationToken = default);
        Task<IGrainBase?> GetOrCreateTypeDefDefaultsAsync(IIdentifiable typeDef, CancellationToken cancellationToken = default);
        Task<IGrainPropDefLocalized?> GetPropDefAsync(Guid id, CultureInfo? culture = null, CancellationToken cancellationToken = default);
        Task<IGrainPropDef?> CreatePropDefAsync(string name, IIdentifiable typeContainer, TraitValueType valueType = TraitValueType.Text, int cardinalityMin = 1, int cardinalityMax = 1, CancellationToken cancellationToken = default);
        Task<int> StorePropDefsAsync(IEnumerable<IGrainPropDef> propdefs, CancellationToken cancellationToken = default);
        Task<IEnumerable<IGrainPropDefLocalized>> GetTypeDefPropertiesAsync(IIdentifiable typedef, CultureInfo? culture = null, CancellationToken cancellationToken = default);
    }
}
using System.Globalization;
using MarBasCommon;
using MarBasSchema.Grain;
using MarBasSchema.GrainDef;

namespace MarBasSchema.Broker
{
    public interface IAsyncGrainDefManagementBroker
    {
        Task<IGrainTypeDefLocalized?> GetTypeDefAsync(Guid id, CultureInfo? culture = null, CancellationToken cancellationToken = default);
        Task<IGrainTypeDef?> CreateTypeDefAsync(string name, IIdentifiable? parent, string? implKey = null, IEnumerable<IIdentifiable>? mixins = null, CancellationToken cancellationToken = default);
        Task<int> StoreGrainTypeDefsAsync(IEnumerable<IGrainTypeDef> typedefs, CancellationToken cancellationToken = default);
        Task<IGrainBase?> GetOrCreateTypeDefDefaultsAsync(IIdentifiable typeDef, CancellationToken cancellationToken = default);
        Task<IGrainPropDefLocalized?> GetPropDefAsync(Guid id, CultureInfo? culture = null, CancellationToken cancellationToken = default);
        Task<IGrainPropDef?> CreatePropDefAsync(string name, IIdentifiable typeContainer, TraitValueType valueType = TraitValueType.Text, int cardinalityMin = 1, int cardinalityMax = 1, CancellationToken cancellationToken = default);
        Task<int> StoreGrainPropDefsAsync(IEnumerable<IGrainPropDef> propdefs, CancellationToken cancellationToken = default);
        Task<IEnumerable<IGrainPropDefLocalized>> GetTypeDefPropertiesAsync(IIdentifiable typedef, CultureInfo? culture = null, CancellationToken cancellationToken = default);
    }
}
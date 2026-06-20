using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;
using CraftedSolutions.MarBasSchema.GrainDef;
using System.Globalization;

namespace CraftedSolutions.MarBasSchema.Broker
{
    public interface IGrainDefManagementBroker
    {
        IGrainTypeDefLocalized? GetTypeDef(Guid id, CultureInfo? culture = null);
        IGrainTypeDef? CreateTypeDef(string name, IIdentifiable? parent, string? implKey = null, IEnumerable<IIdentifiable>? mixins = null);
        int StoreTypeDefs(IEnumerable<IGrainTypeDef> typedefs);
        Type? GetTypeDefTier(IIdentifiable? typeDef);
        IGrainBase? GetOrCreateTypeDefDefaults(IIdentifiable typeDef);
        IGrainPropDefLocalized? GetPropDef(Guid id, CultureInfo? culture = null);
        IGrainPropDef? CreatePropDef(string name, IIdentifiable typeContainer, TraitValueType valueType = TraitValueType.Text, int cardinalityMin = 1, int cardinalityMax = 1);
        int StorePropDefs(IEnumerable<IGrainPropDef> propdefs);
        IEnumerable<IGrainPropDefLocalized> GetTypeDefProperties(IIdentifiable typedef, CultureInfo? culture = null);
    }
}
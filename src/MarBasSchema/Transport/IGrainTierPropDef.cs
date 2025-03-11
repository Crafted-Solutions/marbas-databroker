using System.Text.Json.Serialization;
using CraftedSolutions.MarBasSchema.GrainDef;

namespace CraftedSolutions.MarBasSchema.Transport
{
    [JsonDerivedType(typeof(GrainTierPropDef))]
    public interface IGrainTierPropDef : IGrainTierTransportable, IPropDef
    {
    }
}

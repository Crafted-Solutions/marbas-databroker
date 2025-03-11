using System.Text.Json.Serialization;
using CraftedSolutions.MarBasSchema.GrainDef;

namespace CraftedSolutions.MarBasSchema.Transport
{
    [JsonDerivedType(typeof(GrainTierTypeDef))]
    public interface IGrainTierTypeDef : IGrainTierTransportable, ITypeDef
    {
    }
}

using System.Text.Json.Serialization;
using MarBasSchema.GrainDef;

namespace MarBasSchema.Transport
{
    [JsonDerivedType(typeof(GrainTierTypeDef))]
    public interface IGrainTierTypeDef: IGrainTierTransportable, ITypeDef
    {
    }
}

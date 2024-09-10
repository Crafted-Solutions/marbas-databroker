using System.Text.Json.Serialization;
using MarBasSchema.GrainDef;

namespace MarBasSchema.Transport
{
    [JsonDerivedType(typeof(GrainTierPropDef))]
    public interface IGrainTierPropDef: IGrainTierTransportable, IPropDef
    {
    }
}

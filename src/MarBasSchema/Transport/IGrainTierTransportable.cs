using System.Text.Json.Serialization;
using MarBasSchema.GrainDef;
using MarBasSchema.GrainTier;

namespace MarBasSchema.Transport
{
    [JsonDerivedType(typeof(GrainTierTypeDef), typeDiscriminator: nameof(ITypeDef))]
    [JsonDerivedType(typeof(GrainTierPropDef), typeDiscriminator: nameof(IPropDef))]
    [JsonDerivedType(typeof(GrainTierFile), typeDiscriminator: nameof(IFile))]
    public interface IGrainTierTransportable
    {
    }
}

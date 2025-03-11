using System.Text.Json.Serialization;
using CraftedSolutions.MarBasSchema.GrainDef;
using CraftedSolutions.MarBasSchema.GrainTier;

namespace CraftedSolutions.MarBasSchema.Transport
{
    [JsonDerivedType(typeof(GrainTierTypeDef), typeDiscriminator: nameof(ITypeDef))]
    [JsonDerivedType(typeof(GrainTierPropDef), typeDiscriminator: nameof(IPropDef))]
    [JsonDerivedType(typeof(GrainTierFile), typeDiscriminator: nameof(IFile))]
    public interface IGrainTierTransportable
    {
    }
}

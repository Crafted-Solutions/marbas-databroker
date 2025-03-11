using System.Text.Json.Serialization;
using CraftedSolutions.MarBasSchema.GrainTier;

namespace CraftedSolutions.MarBasSchema.Transport
{
    [JsonDerivedType(typeof(GrainTierFile))]
    public interface IGrainTierFile : IGrainTierTransportable, IFile
    {
    }
}

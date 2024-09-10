using System.Text.Json.Serialization;
using MarBasSchema.GrainTier;

namespace MarBasSchema.Transport
{
    [JsonDerivedType(typeof(GrainTierFile))]
    public interface IGrainTierFile: IGrainTierTransportable, IFile
    {
    }
}

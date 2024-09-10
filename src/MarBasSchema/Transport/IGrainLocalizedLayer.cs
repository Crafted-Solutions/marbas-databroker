using System.Text.Json.Serialization;

namespace MarBasSchema.Transport
{
    [JsonDerivedType(typeof(GrainLocalizedLayer))]
    public interface IGrainLocalizedLayer
    {
        string? Label { get; set; }
        IEnumerable<ITraitTransportable>? Traits { get; set; }
    }
}

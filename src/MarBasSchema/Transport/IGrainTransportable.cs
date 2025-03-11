using System.Text.Json.Serialization;
using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasSchema.Transport
{
    [JsonDerivedType(typeof(GrainTransportable))]
    public interface IGrainTransportable : IGrain
    {
        IGrainTierTransportable? Tier { get; set; }
        IEnumerable<IAclEntryTransportable> Acl { get; set; }
        IEnumerable<ITraitTransportable>? Traits { get; set; }
        IDictionary<string, IGrainLocalizedLayer> Localized { get; set; }
    }
}

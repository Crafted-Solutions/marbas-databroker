
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasSchema.Transport
{
    public class GrainTransportable : GrainPlain, IGrainTransportable
    {
        [JsonConstructor]
        public GrainTransportable()
        {
        }

        public GrainTransportable(IGrain other)
           : base(other)
        {
            if (other is IGrainTransportable transportable)
            {
                Tier = transportable.Tier;
                Acl = transportable.Acl;
                Traits = transportable.Traits;
                Localized = transportable.Localized;
            }
        }

        public IGrainTierTransportable? Tier { get; set; }
        public IEnumerable<IAclEntryTransportable> Acl { get; set; } = Enumerable.Empty<IAclEntryTransportable>();
        public IEnumerable<ITraitTransportable>? Traits { get; set; }
        public IDictionary<string, IGrainLocalizedLayer> Localized { get; set; } = ImmutableDictionary<string, IGrainLocalizedLayer>.Empty;
    }
}

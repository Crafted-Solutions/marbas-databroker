
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasSchema.GrainDef;

namespace CraftedSolutions.MarBasSchema.Transport
{
    public class GrainTierTypeDef : IGrainTierTypeDef
    {
        [JsonConstructor]
        public GrainTierTypeDef()
        {
        }

        public GrainTierTypeDef(ITypeDef other)
        {
            Impl = other.Impl;
            MixInIds = other.MixInIds;
        }

        public string? Impl { get; set; }

        public IEnumerable<Guid> MixInIds { get; set; } = Enumerable.Empty<Guid>();
    }
}

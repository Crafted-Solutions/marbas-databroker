using System.Collections.Immutable;
using CraftedSolutions.MarBasSchema.Transport;

namespace CraftedSolutions.MarBasAPICore.Models.Transport
{
    public sealed class GrainImportModel : IGrainImportModel
    {
        public ISet<GrainTransportable> Grains { get; set; } = ImmutableHashSet<GrainTransportable>.Empty;
        public ISet<Guid>? GrainsToDelete { get; set; }
        public DuplicatesHandlingStrategy? DuplicatesHandling { get; set; }
    }
}

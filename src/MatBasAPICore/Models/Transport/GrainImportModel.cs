using System.Collections.Immutable;
using MarBasSchema;
using MarBasSchema.Transport;

namespace MarBasAPICore.Models.Transport
{
    public sealed class GrainImportModel : IGrainImportModel
    {
        public ISet<GrainTransportable> Grains { get; set; } = ImmutableHashSet<GrainTransportable>.Empty;
        public ISet<Guid>? GrainsToDelete { get; set; }
        public DuplicatesHandlingStrategy? DuplicatesHandling { get; set; }
    }
}

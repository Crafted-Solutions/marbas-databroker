using CraftedSolutions.MarBasSchema.Transport;
using System.Collections.Immutable;

namespace CraftedSolutions.MarBasAPICore.Models.Transport
{

    public sealed class PackageExportModel : IPackageExportModel
    {
        public string? NamePrefix { get; set; }
        public IDictionary<Guid, IGrainPackagingOptions> Items { get; set; } = ImmutableDictionary<Guid, IGrainPackagingOptions>.Empty;
    }
}

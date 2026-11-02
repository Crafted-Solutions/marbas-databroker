using CraftedSolutions.MarBasSchema.Transport;
using System.Text.Json.Serialization;

namespace CraftedSolutions.MarBasAPICore.Models.Transport
{

    [JsonDerivedType(typeof(PackageExportModel))]
    public interface IPackageExportModel
    {
        string? NamePrefix { get; set; }
        IDictionary<Guid, IGrainPackagingOptions> Items { get; set; }
    }
}

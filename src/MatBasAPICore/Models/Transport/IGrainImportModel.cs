using System.Text.Json.Serialization;
using MarBasSchema;
using MarBasSchema.Transport;

namespace MarBasAPICore.Models.Transport
{
    [JsonDerivedType(typeof(GrainImportModel))]
    public interface IGrainImportModel
    {
        ISet<GrainTransportable> Grains { get; set; }
        ISet<Guid>? GrainsToDelete { get; set; }
        DuplicatesHandlingStrategy? DuplicatesHandling {  get; set; }
    }
}

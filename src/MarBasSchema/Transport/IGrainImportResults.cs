using System.Text.Json.Serialization;
using MarBasSchema.Broker;

namespace MarBasSchema.Transport
{
    [JsonDerivedType(typeof(GrainImportResults))]
    public interface IGrainImportResults
    {
        long ImportedCount { get; }
        long IgnoredCount { get; }
        long DeletedCount { get; }
        IEnumerable<IBrokerOperationFeedback>? Feedback { get; }
    }
}

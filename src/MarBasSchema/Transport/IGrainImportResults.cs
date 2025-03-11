using System.Text.Json.Serialization;
using CraftedSolutions.MarBasSchema.Broker;

namespace CraftedSolutions.MarBasSchema.Transport
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

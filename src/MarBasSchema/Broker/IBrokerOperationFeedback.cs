using Microsoft.Extensions.Logging;

namespace CraftedSolutions.MarBasSchema.Broker
{
    public interface IBrokerOperationFeedback
    {
        LogLevel FeedbackType { get; }
        string Source { get; }
        int Code { get; }
        string Message { get; }
        Guid? ObjectId { get; }
    }
}

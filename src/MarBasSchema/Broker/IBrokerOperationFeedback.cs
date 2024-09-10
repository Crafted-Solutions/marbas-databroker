using Microsoft.Extensions.Logging;

namespace MarBasSchema.Broker
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

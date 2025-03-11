
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace CraftedSolutions.MarBasSchema.Broker
{
    public class BrokerOperationFeedback : IBrokerOperationFeedback
    {
        [JsonConstructor]
        public BrokerOperationFeedback() { }

        public BrokerOperationFeedback(string message, string? source = null, int code = 0, LogLevel type = LogLevel.Information, Guid? objectId = null)
        {
            if (null != source)
            {
                Source = source;
            }
            Code = code;
            Message = message;
            ObjectId = objectId;
            FeedbackType = type;
        }

        public string Source { get; set; } = "General";

        public int Code { get; set; } = -1;

        public string Message { get; set; } = string.Empty;

        public Guid? ObjectId { get; set; }

        public LogLevel FeedbackType { get; set; } = LogLevel.Information;
    }
}

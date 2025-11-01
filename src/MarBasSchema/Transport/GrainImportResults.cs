using CraftedSolutions.MarBasSchema.Broker;

namespace CraftedSolutions.MarBasSchema.Transport
{
    public sealed class GrainImportResults : IGrainImportResults
    {
        private readonly object _sync = new();
        private IList<IBrokerOperationFeedback>? _feedback;

        public long ImportedCount { get; set; }

        public long IgnoredCount { get; set; }

        public long DeletedCount { get; set; }

        public IEnumerable<IBrokerOperationFeedback>? Feedback { get => _feedback; set => _feedback = value?.ToList(); }

        public void AddFeedback(IBrokerOperationFeedback item)
        {
            lock (_sync)
            {
                if (null == _feedback)
                {
                    _feedback = [];
                }
                _feedback.Add(item);
            }

        }

        public static GrainImportResults Merge(IGrainImportResults first, IGrainImportResults second)
        {
            return new ()
            {
                ImportedCount = first.ImportedCount + second.ImportedCount,
                IgnoredCount = first.IgnoredCount + second.IgnoredCount,
                DeletedCount = first.DeletedCount + second.DeletedCount,
                Feedback = null == second.Feedback ? first.Feedback : first.Feedback?.Concat(second.Feedback) ?? second.Feedback
            };
        }
    }
}

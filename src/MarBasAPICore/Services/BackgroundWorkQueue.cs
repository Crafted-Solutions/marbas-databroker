using CraftedSolutions.MarBasCommon.Job;
using Microsoft.Extensions.Configuration;
using System.Threading.Channels;

namespace CraftedSolutions.MarBasAPICore.Services
{
    public sealed class BackgroundWorkQueue : IBackgroundWorkQueue
    {
        private readonly Channel<Func<CancellationToken, ValueTask>> _queue;

        public BackgroundWorkQueue(IConfiguration configuration)
        {
            var options = new BoundedChannelOptions(configuration.GetValue("BackgroundJobs:Capacity", 100))
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
        }

        public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken)
        {
            var workItem = await _queue.Reader.ReadAsync(cancellationToken);
            return workItem;
        }

        public async ValueTask QueueWorkItemAsync(Func<CancellationToken, ValueTask> workItem)
        {
            ArgumentNullException.ThrowIfNull(workItem);
            await _queue.Writer.WriteAsync(workItem);
        }
    }
}

namespace CraftedSolutions.MarBasCommon.Job
{
    public interface IBackgroundWorkQueue
    {
        ValueTask QueueWorkItemAsync(Func<CancellationToken, ValueTask> workItem);
        ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
    }
}

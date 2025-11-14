namespace CraftedSolutions.MarBasCommon.Job
{
    public interface IBackgroundJobManager
    {
        IBackgroundJob? GetJob(Guid id, bool removeIfStopped = false);
        IBackgroundJob EmplaceJob(string name, string? owner = null, BackgroundJobFlags flags = BackgroundJobFlags.None, string stage = "Default");
        IBackgroundJob AddJob(IBackgroundJob newJob);
        IBackgroundJob? RemoveJob(Guid id, bool cancel = false);
        IEnumerable<IBackgroundJob> ListJobs(bool forAllUsers = false);
    }
}

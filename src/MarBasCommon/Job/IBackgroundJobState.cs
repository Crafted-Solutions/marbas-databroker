namespace CraftedSolutions.MarBasCommon.Job
{
    public enum BackgroundJobStatus
    {
        Pending, Running, Paused, Complete, Cancelled, Error
    }

    public interface IBackgroundJobState
    {
        string Stage { get; set; }
        int Progress { get; set; }
        BackgroundJobStatus Status { get; set; }
        object? Result { get; set; }
    }
}

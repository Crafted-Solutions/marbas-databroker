using System.ComponentModel;

namespace CraftedSolutions.MarBasCommon.Job
{
    [Flags]
    public enum BackgroundJobFlags
    {
        None = 0x0, Pausable = 1 << 0, Critical = 1 << 1
    }

    public interface IBackgroundJob
        : IIdentifiable, INamed, IBackgroundJobState, INotifyPropertyChanged, IDisposable
    {
        string Owner { get; }
        BackgroundJobFlags Flags { get; }
        DateTime Created { get; }
        DateTime? Started { get; }
        void RegisterForDispose(IDisposable disposable);
    }
}

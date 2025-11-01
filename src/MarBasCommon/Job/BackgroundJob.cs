using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CraftedSolutions.MarBasCommon.Job
{
    public class BackgroundJob(string name, string owner, BackgroundJobFlags flags = BackgroundJobFlags.None, string stage = "Default")
        : IBackgroundJob
    {
        private readonly Guid _id = Guid.NewGuid();
        private readonly string _name = name;
        private readonly string _owner = owner;
        private readonly BackgroundJobFlags _flags = flags;
        private readonly DateTime _created = DateTime.UtcNow;
        private readonly object _lock = new();
        private readonly List<IDisposable> _disposables = [];

        private volatile string _stage = stage;
        private volatile object? _result;
        private BackgroundJobStatus _status = BackgroundJobStatus.Pending;
        private int _progress = -1;
        private DateTime _started;
        private bool _disposed;

        public Guid Id => _id;

        public string Name => _name;

        public string Owner => _owner;

        public BackgroundJobFlags Flags => _flags;

        public DateTime Created => _created;

        public string Stage
        {
            get => _stage;
            set
            {
                _stage = value;
                NotifyPropertyChanged();
            }
        }

        public int Progress
        {
            get => Interlocked.CompareExchange(ref _progress, 0, 0);
            set
            {
                Interlocked.Exchange(ref _progress, value);
                NotifyPropertyChanged();
            }
        }

        public BackgroundJobStatus Status
        {
            get
            {
                lock (_lock) return _status;
            }
            set
            {
                lock ( _lock )
                {
                    var prev = _status;
                    _status = value;
                    if (BackgroundJobStatus.Pending == prev && BackgroundJobStatus.Pending < value)
                    {
                        _started = DateTime.UtcNow;
                    }
                    if (BackgroundJobStatus.Complete <= value)
                    {
                        Dispose();
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public object? Result
        {
            get => _result;
            set
            {
                _result = value;
                NotifyPropertyChanged();
            }
        }

        public DateTime? Started
        {
            get
            {
                lock (_lock) return BackgroundJobStatus.Pending < Status ? _started : null;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Dispose()
        {
            lock(_lock)
            {
                if (!_disposed)
                {
                    foreach (var d in _disposables) { d.Dispose(); }
                    _disposables.Clear();
                    _disposed = true;
                }
            }
            GC.SuppressFinalize(this);
        }

        public void RegisterForDispose(IDisposable disposable)
        {
            lock(_lock)
            {
                _disposables.Add(disposable);
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class Context: IBackgroudJobContext
        {
            private readonly IBackgroundJob _backgroundJob;
            private readonly CancellationTokenSource _cts;

            public Context(IBackgroundJob backgroundJob, CancellationToken cancellationToken = default)
            {
                if (BackgroundJobStatus.Cancelled == backgroundJob.Status)
                {
                    throw new OperationCanceledException($"Job {backgroundJob.Id} has been cancelled prior to starting work");
                }
                _backgroundJob = backgroundJob;
                _cts = EqualityComparer<CancellationToken>.Default.Equals(cancellationToken, default)
                    ? new CancellationTokenSource() : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                _backgroundJob.PropertyChanged += (sender, args) =>
                {
                    if (nameof(IBackgroundJobState.Status) == args.PropertyName && sender == _backgroundJob && BackgroundJobStatus.Cancelled == _backgroundJob.Status)
                    {
                        _cts.Cancel();
                    }
                };
            }

            public CancellationToken CancellationToken => _cts.Token;

            public string Stage { get => _backgroundJob.Stage; set => _backgroundJob.Stage = value; }
            public int Progress { get => _backgroundJob.Progress; set => _backgroundJob.Progress = value; }
            public BackgroundJobStatus Status
            {
                get => _cts.IsCancellationRequested ? BackgroundJobStatus.Cancelled : _backgroundJob.Status;
                set => _backgroundJob.Status = value;
            }
            public object? Result { get => _backgroundJob.Result; set => _backgroundJob.Result = value; }

        }
    }
}

using Nito.AsyncEx;

namespace CraftedSolutions.MarBasBrokerEngineSQLite
{
    public interface ISchemaLock
    {
        IDisposable ReaderLock();
        IDisposable WriterLock();
    }

    public interface IAsyncSchemaLock
    {
        AwaitableDisposable<IDisposable> ReaderLockAsync(CancellationToken cancellationToken);
        AwaitableDisposable<IDisposable> WriterLockAsync(CancellationToken cancellationToken);
    }
}
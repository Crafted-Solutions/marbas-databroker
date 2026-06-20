using System.Data.Common;

namespace CraftedSolutions.MarBasBrokerSQLCommon.Lob
{
    public interface IBlobContext : IDisposable
    {
        [Obsolete("Command property is deprecated, use ExecuteOnConnection method instead")]
        DbCommand Command { get; }
        [Obsolete("GetCommandAsync method is deprecated, use ExecuteOnConnection method instead")]
        Task<DbCommand> GetCommandAsync(CancellationToken cancellationToken = default);
        Task<T> ExecuteOnConnection<T>(Func<DbCommand, Task<T>> func, CancellationToken cancellationToken = default);
        DbConnection Connection { get; }
        string DataColumn { get; }
    }
}

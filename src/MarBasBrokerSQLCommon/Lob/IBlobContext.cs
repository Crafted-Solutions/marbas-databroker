using System.Data.Common;

namespace MarBasBrokerSQLCommon.Lob
{
    public interface IBlobContext : IDisposable
    {
        DbCommand Command { get; }
        Task<DbCommand> GetCommandAsync(CancellationToken cancellationToken = default);
        DbConnection Connection { get; }
        string DataColumn { get; }
    }
}

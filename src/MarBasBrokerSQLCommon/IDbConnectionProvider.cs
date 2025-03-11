using System.Data.Common;

namespace CraftedSolutions.MarBasBrokerSQLCommon
{
    public interface IDbConnectionProvider
    {
        DbConnection Connection { get; }
    }

    public interface IDbConnectionProvider<TConn> : IDbConnectionProvider where TConn : DbConnection
    {
        new TConn Connection { get; }
    }
}

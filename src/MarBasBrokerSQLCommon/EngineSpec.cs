namespace CraftedSolutions.MarBasBrokerSQLCommon
{
    public class EngineSpec<TDialect> where TDialect : ISQLDialect, new()
    {
        public static readonly TDialect Dialect = new TDialect();
    }
}

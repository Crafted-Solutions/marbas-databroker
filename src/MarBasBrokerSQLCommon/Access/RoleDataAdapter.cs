using System.Data.Common;
using MarBasSchema.Access;

namespace MarBasBrokerSQLCommon.Access
{
    public class RoleDataAdapter : AbstractDataAdapter, ISchemaRole
    {
        public RoleDataAdapter(DbDataReader dataReader) : base(dataReader)
        {
        }

        public RoleEntitlement Entitlement { get => (RoleEntitlement)(UInt32)_dataReader.GetInt32(_dataReader.GetOrdinal(GetMappedColumnName())); set => throw new NotImplementedException(); }

        public Guid Id => GetGuid(GetMappedColumnName());

        public string Name => _dataReader.GetString(_dataReader.GetOrdinal(GetMappedColumnName()));
    }
}

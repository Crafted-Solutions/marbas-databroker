using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using CraftedSolutions.MarBasBrokerSQLCommon;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Access;

namespace CraftedSolutions.MarBasBrokerSQLCommon.Access
{
    public class AclDataAdapter : AbstractDataAdapter, ISchemaAclEntry
    {
        [Flags]
        public enum ExtensionColumn
        {
            None = 0, SourceGrain = 1, All = SourceGrain
        }

        protected readonly ExtensionColumn _extensionColumn;

        public AclDataAdapter(DbDataReader dataReader, ExtensionColumn extensionColumn = ExtensionColumn.All)
            : base(dataReader)
        {
            _extensionColumn = extensionColumn;
        }

        [Column(AclDefaults.FieldRoleId)]
        public Guid RoleId => Role.Id;
        [Column(AclDefaults.FieldRoleId)]
        public IIdentifiable Role { get => (Identifiable)GetGuid(GetMappedColumnName()); set => throw new NotImplementedException(); }

        [Column(GeneralEntityDefaults.FieldGrainId)]
        public Guid GrainId => Grain.Id;
        [Column(GeneralEntityDefaults.FieldGrainId)]
        public IIdentifiable Grain { get => (Identifiable)GetGuid(GetMappedColumnName()); set => throw new NotImplementedException(); }

        public bool Inherit { get => _dataReader.GetBoolean(_dataReader.GetOrdinal(GetMappedColumnName())); set => throw new NotImplementedException(); }
        [Column(AclDefaults.FieldPermissionMask)]
        public GrainAccessFlag PermissionMask { get => (GrainAccessFlag)(uint)_dataReader.GetInt64(_dataReader.GetOrdinal(GetMappedColumnName())); set => throw new NotImplementedException(); }
        [Column(AclDefaults.FieldRestrictionMask)]
        public GrainAccessFlag RestrictionMask { get => (GrainAccessFlag)(uint)_dataReader.GetInt64(_dataReader.GetOrdinal(GetMappedColumnName())); set => throw new NotImplementedException(); }

        [Column(AclDefaults.FieldSourceGrain)]
        public Guid? SourceGrainId => SourceGrain?.Id;
        [Column(AclDefaults.FieldSourceGrain)]
        public IIdentifiable? SourceGrain
        {
            get
            {
                if (ExtensionColumn.SourceGrain == (ExtensionColumn.SourceGrain & _extensionColumn))
                {
                    return (Identifiable?)GetNullableGuid(GetMappedColumnName());
                }
                return null;
            }
        }
    }
}

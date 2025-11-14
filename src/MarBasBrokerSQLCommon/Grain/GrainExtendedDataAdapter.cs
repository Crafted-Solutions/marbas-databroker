using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Grain;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;

namespace CraftedSolutions.MarBasBrokerSQLCommon.Grain
{
    public class GrainExtendedDataAdapter(DbDataReader dataReader, GrainExtendedDataAdapter.ExtensionColumn extensionColumn = GrainExtendedDataAdapter.ExtensionColumn.All)
        : AbstractDataAdapter(dataReader), IGrainExtended
    {
        [Flags]
        public enum ExtensionColumn
        {
            None = 0, Type = 1, Path = 2, Permissions = 4, Container = 8, All = Type | Path | Permissions | Container
        }

        protected readonly ExtensionColumn _extensionColumn = extensionColumn;

        public Guid Id => GetGuid(GetMappedColumnName());

        [Column(GrainBaseConfig.FieldParentId)]
        public IIdentifiable? Parent { get => (Identifiable?)GetNullableGuid(GetMappedColumnName()); set => throw new NotImplementedException(); }
        [Column(GrainBaseConfig.FieldParentId)]
        public Guid? ParentId { get => Parent?.Id; }
        [Column(GrainBaseConfig.FieldTypeDefId)]
        public IIdentifiable? TypeDef
        {
            get
            {
                var id = GetNullableGuid(GetMappedColumnName());
                return null == id ? null : new NamedIdentifiable((Guid)id, TypeName);
            }
            set => throw new NotImplementedException();
        }

        [Column(GrainBaseConfig.FieldTypeDefId)]
        public Guid? TypeDefId { get => TypeDef?.Id; }

        public string Name { get => _dataReader.GetString(_dataReader.GetOrdinal(GetMappedColumnName())); set => throw new NotImplementedException(); }

        public DateTime CTime => GetDateTime(GetMappedColumnName());

        public DateTime MTime => GetDateTime(GetMappedColumnName());

        public string Owner
        {
            get
            {
                var ord = _dataReader.GetOrdinal(GetMappedColumnName());
                if (_dataReader.IsDBNull(ord))
                {
                    return SchemaDefaults.AnonymousUserName;
                }
                return _dataReader.GetString(ord);
            }
        }

        public int Revision { get => _dataReader.GetInt32(_dataReader.GetOrdinal(GetMappedColumnName())); set => throw new NotImplementedException(); }

        [Column("sort_key")]
        public string? SortKey { get => GetNullableField<string>(GetMappedColumnName()); set => throw new NotImplementedException(); }

        [Column("xattrs")]
        public string? XAttrs { get => GetNullableField<string>(GetMappedColumnName()); set => throw new NotImplementedException(); }

        [Column("custom_flag")]
        public int CustomFlag { get => _dataReader.GetInt32(_dataReader.GetOrdinal(GetMappedColumnName())); set => throw new NotImplementedException(); }

        [Column("type_name")]
        public string? TypeName
        {
            get
            {
                if (_extensionColumn.HasFlag(ExtensionColumn.Type))
                {
                    return GetNullableField<string>(GetMappedColumnName());
                }
                return null;
            }
        }

        [Column("type_xattrs")]
        public string? TypeXAttrs
        {
            get
            {
                if (_extensionColumn.HasFlag(ExtensionColumn.Type))
                {
                    return GetNullableField<string>(GetMappedColumnName());
                }
                return null;
            }
        }

        public string? Path
        {
            get
            {
                if (_extensionColumn.HasFlag(ExtensionColumn.Path))
                {
                    return _dataReader.GetString(_dataReader.GetOrdinal(GetMappedColumnName()));
                }
                return null;
            }
        }

        public GrainAccessFlag Permissions
        {
            get
            {
                if (_extensionColumn.HasFlag(ExtensionColumn.Permissions))
                {
                    return (GrainAccessFlag)(uint)_dataReader.GetInt64(_dataReader.GetOrdinal(GetMappedColumnName()));
                }
                return GrainAccessFlag.None;
            }
        }

        [Column("child_count")]
        public int ChildCount
        {
            get
            {
                if (_extensionColumn.HasFlag(ExtensionColumn.Container))
                {
                    return _dataReader.GetInt32(_dataReader.GetOrdinal(GetMappedColumnName()));
                }
                return 0;
            }
        }

    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Globalization;
using CraftedSolutions.MarBasBrokerSQLCommon.GrainDef;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Grain;
using CraftedSolutions.MarBasSchema.Grain.Traits;

namespace CraftedSolutions.MarBasBrokerSQLCommon.Grain
{
    public class TraitBaseDataAdapter : AbstractDataAdapter, ITraitBase
    {

        public static TraitBaseDataAdapter Create(DbDataReader dataReader, string? valueType = null)
        {

            var result = new TraitBaseDataAdapter(dataReader, valueType);
            switch (result.ValueType)
            {
                case TraitValueType.Memo:
                case TraitValueType.Text:
                    result = new TraitTextAdapter(dataReader, result.ValueType);
                    break;
                case TraitValueType.Number:
                    result = new TraitNumberAdapter(dataReader);
                    break;
                case TraitValueType.Boolean:
                    result = new TraitBoolAdapter(dataReader);
                    break;
                case TraitValueType.DateTime:
                    result = new TraitDateTimeAdapter(dataReader);
                    break;
                case TraitValueType.Grain:
                    result = new TraitGrainAdapter(dataReader);
                    break;
                case TraitValueType.File:
                    result = new TraitFileAdapter(dataReader);
                    break;
            }
            return result;
        }

        public static string GetValueColumn(TraitValueType valueType)
        {
            return valueType switch
            {
                TraitValueType.Grain or TraitValueType.File => "val_guid",
                _ => $"val_{(Enum.GetName(valueType) ?? Enum.GetName(TraitValueType.Text))!.ToLowerInvariant()}",
            };
        }

        protected readonly int _valueColumn;
        protected TraitValueType _valueType;

        public TraitBaseDataAdapter(DbDataReader dataReader, string? valueType = null) : base(dataReader)
        {
            _valueType = TraitValueFactory.GetValueTypeFromString(valueType ?? GetNullableField<string>(GetMappedColumnName(nameof(ITraitBase.ValueType))));
            _valueColumn = _dataReader.GetOrdinal(GetValueColumn(_valueType));
        }

        public TraitBaseDataAdapter(DbDataReader dataReader, TraitValueType valueType) : base(dataReader)
        {
            _valueType = valueType;
            _valueColumn = _dataReader.GetOrdinal(GetValueColumn(_valueType));
        }

        public Guid Id => GetGuid(GetMappedColumnName());

        [Column(GeneralEntityDefaults.FieldGrainId)]
        public IIdentifiable Grain { get => (Identifiable)GetGuid(GetMappedColumnName()); set => throw new NotImplementedException(); }

        [Column(GeneralEntityDefaults.FieldGrainId)]
        public Guid GrainId => GetGuid(GetMappedColumnName());

        [Column(TraitBaseDefaults.FieldPropDefId)]
        public IIdentifiable PropDef { get => (Identifiable)GetGuid(GetMappedColumnName()); set => throw new NotImplementedException(); }

        [Column(TraitBaseDefaults.FieldPropDefId)]
        public Guid PropDefId => GetGuid(GetMappedColumnName());

        public int Ord { get => _dataReader.GetInt32(_dataReader.GetOrdinal(GetMappedColumnName())); set => throw new NotImplementedException(); }
        public int Revision { get => _dataReader.GetInt32(_dataReader.GetOrdinal(GetMappedColumnName())); set => throw new NotImplementedException(); }

        public bool IsNull => null != Value;

        public object? Value => _dataReader.GetValue(_valueColumn);

        [Column(GeneralEntityDefaults.FieldLangCode)]
        public CultureInfo? CultureInfo
        {
            get
            {
                var ord = _dataReader.GetOrdinal(GetMappedColumnName());
                return (_dataReader.IsDBNull(ord) ? null : CultureInfo.GetCultureInfo(_dataReader.GetString(ord)))!;
            }
        }

        [Column(GeneralEntityDefaults.FieldLangCode)]
        public string? Culture => CultureInfo?.IetfLanguageTag;


        [Column(GrainPropDefDefaults.FieldValueType)]
        public TraitValueType ValueType => _valueType;

        public virtual ITraitBase? Adapt() => null;

        public class ColumnMapper : IColumnMapper
        {
            public string? GetColumnName(string fieldName, IUpdateable updateable)
            {
                if (nameof(ITraitBase.Value) == fieldName && updateable is ITraitBase trait)
                {
                    return GetValueColumn(trait.ValueType);
                }
                return null;
            }
        }

        public sealed class TraitTextAdapter : TraitBaseDataAdapter, ITraitValue<string>
        {
            public TraitTextAdapter(DbDataReader dataReader, TraitValueType valueType = TraitValueType.Text) : base(dataReader, valueType)
            {
            }

            string? ITraitValue<string>.Value { get => _dataReader.IsDBNull(_valueColumn) ? null : _dataReader.GetString(_valueColumn); set => throw new NotImplementedException(); }

            public override ITraitBase? Adapt()
            {
                return new TraitText(this);
            }
        }

        public sealed class TraitNumberAdapter : TraitBaseDataAdapter, ITraitValue<decimal?>
        {
            public TraitNumberAdapter(DbDataReader dataReader) : base(dataReader, TraitValueType.Number)
            {
            }

            decimal? ITraitValue<decimal?>.Value { get => _dataReader.IsDBNull(_valueColumn) ? null : _dataReader.GetDecimal(_valueColumn); set => throw new NotImplementedException(); }

            public override ITraitBase? Adapt()
            {
                return new TraitValue<decimal?>(this);
            }
        }

        public sealed class TraitBoolAdapter : TraitBaseDataAdapter, ITraitValue<bool>
        {
            public TraitBoolAdapter(DbDataReader dataReader) : base(dataReader, TraitValueType.Boolean)
            {
            }

            bool ITraitValue<bool>.Value { get => !_dataReader.IsDBNull(_valueColumn) && _dataReader.GetBoolean(_valueColumn); set => throw new NotImplementedException(); }

            public override ITraitBase? Adapt()
            {
                return new TraitValue<bool>(this);
            }
        }

        public sealed class TraitDateTimeAdapter : TraitBaseDataAdapter, ITraitValue<DateTime?>
        {
            public TraitDateTimeAdapter(DbDataReader dataReader) : base(dataReader, TraitValueType.DateTime)
            {
            }

            DateTime? ITraitValue<DateTime?>.Value { get => _dataReader.IsDBNull(_valueColumn) ? null : _dataReader.GetDateTime(_valueColumn); set => throw new NotImplementedException(); }

            public override ITraitBase? Adapt()
            {
                return new TraitValue<DateTime?>(this);
            }
        }

        public class TraitGrainAdapter : TraitBaseDataAdapter, ITraitValue<Guid?>
        {
            public TraitGrainAdapter(DbDataReader dataReader, TraitValueType valueType = TraitValueType.Grain) : base(dataReader, valueType)
            {
            }

            Guid? ITraitValue<Guid?>.Value { get => _dataReader.IsDBNull(_valueColumn) ? null : _dataReader.GetGuid(_valueColumn); set => throw new NotImplementedException(); }

            public override ITraitBase? Adapt()
            {
                return new TraitValue<Guid?>(this);
            }
        }

        public sealed class TraitFileAdapter : TraitGrainAdapter
        {
            public TraitFileAdapter(DbDataReader dataReader) : base(dataReader, TraitValueType.File)
            {
            }

            public override ITraitBase? Adapt()
            {
                return new TraitFile(this);
            }
        }
    }
}

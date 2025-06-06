﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using CraftedSolutions.MarBasBrokerSQLCommon.Grain;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Grain.Traits;
using CraftedSolutions.MarBasSchema.GrainDef;

namespace CraftedSolutions.MarBasBrokerSQLCommon.GrainDef
{
    public class GrainPropDefDataAdapter : GrainLocalizedDataAdapter, IGrainPropDef
    {
        public GrainPropDefDataAdapter(DbDataReader dataReader) : base(dataReader)
        {
        }

        [Column(GrainPropDefDefaults.FieldValueType)]
        public TraitValueType ValueType { get => TraitValueFactory.GetValueTypeFromString(GetNullableField<string>(GetMappedColumnName())); set => throw new NotImplementedException(); }

        [Column(GrainPropDefDefaults.FieldValueConstraint)]
        public IIdentifiable? ValueConstraint { get => (Identifiable?)GetNullableGuid(GetMappedColumnName()); set => throw new NotImplementedException(); }

        [Column(GrainPropDefDefaults.FieldValueConstraint)]
        public Guid? ValueConstraintId { get => ValueConstraint?.Id; set => throw new NotImplementedException(); }

        [Column("constraint_params")]
        public string? ConstraintParams { get => GetNullableField<string>(GetMappedColumnName()); set => throw new NotImplementedException(); }

        [Column(GrainPropDefDefaults.FieldCardinalityMin)]
        public int CardinalityMin { get => _dataReader.GetInt32(_dataReader.GetOrdinal(GetMappedColumnName())); set => throw new NotImplementedException(); }

        [Column(GrainPropDefDefaults.FieldCardinalityMax)]
        public int CardinalityMax { get => _dataReader.GetInt32(_dataReader.GetOrdinal(GetMappedColumnName())); set => throw new NotImplementedException(); }

        public bool Versionable { get => _dataReader.GetBoolean(_dataReader.GetOrdinal(GetMappedColumnName())); set => throw new NotImplementedException(); }

        public bool Localizable { get => _dataReader.GetBoolean(_dataReader.GetOrdinal(GetMappedColumnName())); set => throw new NotImplementedException(); }

        [Column(GrainPropDefDefaults.FieldValueType)]
        TraitValueType IValueTypeConstraint.ValueType => ValueType;

        public sealed class FieldValueMapper : IFieldValueMapper
        {
            public Type? GetFieldType(string fieldName)
            {
                if (nameof(IGrainPropDef.ValueType).Equals(fieldName, StringComparison.Ordinal))
                {
                    return typeof(string);
                }
                return null;
            }

            public object? MapFieldValue(string fieldName, object? origValue)
            {
                if (nameof(IGrainPropDef.ValueType).Equals(fieldName, StringComparison.Ordinal))
                {
                    return TraitValueFactory.GetValueTypeAsString((dynamic?)origValue);
                }
                return origValue;
            }
        }
    }
}

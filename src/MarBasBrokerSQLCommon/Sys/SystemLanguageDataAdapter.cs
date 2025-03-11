using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Globalization;
using CraftedSolutions.MarBasSchema.Sys;

namespace CraftedSolutions.MarBasBrokerSQLCommon.Sys
{
    public class SystemLanguageDataAdapter : AbstractDataAdapter, ISystemLanguage
    {
        public SystemLanguageDataAdapter(DbDataReader dataReader) : base(dataReader)
        {
        }

        public string Label { get => _dataReader.GetString(_dataReader.GetOrdinal(GetMappedColumnName())); set => throw new NotImplementedException(); }
        [Column(SystemLanguageDefaults.FieldLabelNative)]
        public string? LabelNative { get => GetNullableField<string>(GetMappedColumnName()); set => throw new NotImplementedException(); }
        [Column(SystemLanguageDefaults.FieldIsoCode)]
        public string IsoCode { get => _dataReader.GetString(_dataReader.GetOrdinal(GetMappedColumnName())); set => throw new NotImplementedException(); }

        public CultureInfo ToCultureInfo() => new SystemLanguage(this).ToCultureInfo();
    }
}

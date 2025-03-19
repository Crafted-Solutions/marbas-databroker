using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Globalization;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasBrokerSQLCommon.Grain
{
    public class GrainLabelAdapter(DbDataReader dataReader) : AbstractDataAdapter(dataReader), IGrainLabel
    {
        [Column(name: GeneralEntityDefaults.FieldLangCode)]
        public CultureInfo CultureInfo
        {
            get
            {
                var ord = _dataReader.GetOrdinal(GetMappedColumnName());
                var val = _dataReader.IsDBNull(ord) ? null : _dataReader.GetString(ord);
                return (null == val || "~" == val ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(val))!;
            }
        }
        [Column(name: GeneralEntityDefaults.FieldLangCode)]
        string ILocalized.Culture => CultureInfo.IetfLanguageTag;
        [Column(name: GeneralEntityDefaults.FieldLangCode)]
        string? ILocalizable.Culture => CultureInfo.IetfLanguageTag;

        public string? Label
        {
            get
            {
                var ord = _dataReader.GetOrdinal(GetMappedColumnName());
                return _dataReader.IsDBNull(ord) ? null : _dataReader.GetString(ord);
            }
            set => throw new NotImplementedException();
        }

        [Column(GeneralEntityDefaults.FieldGrainId)]
        public IIdentifiable Grain { get => (Identifiable)GetGuid(GetMappedColumnName()); set => throw new NotImplementedException(); }

        [Column(GeneralEntityDefaults.FieldGrainId)]
        public Guid GrainId => GetGuid(GetMappedColumnName());
    }
}

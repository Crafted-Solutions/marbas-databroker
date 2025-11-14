using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Globalization;

namespace CraftedSolutions.MarBasBrokerSQLCommon.Grain
{
    public class GrainLocalizedDataAdapter(DbDataReader dataReader) : GrainExtendedDataAdapter(dataReader), IGrainLocalized
    {
        [Column(name: GeneralEntityDefaults.FieldLangCode)]
        public CultureInfo CultureInfo
        {
            get
            {
                var ord = _dataReader.GetOrdinal(GetMappedColumnName());
                return (_dataReader.IsDBNull(ord) ? null : CultureInfo.GetCultureInfo(_dataReader.GetString(ord)))!;
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

    }
}

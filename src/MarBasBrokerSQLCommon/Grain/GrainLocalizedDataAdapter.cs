using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Globalization;
using CraftedSolutions.MarBasBrokerSQLCommon;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasBrokerSQLCommon.Grain
{
    public class GrainLocalizedDataAdapter : GrainExtendedDataAdapter, IGrainLocalized
    {

        public GrainLocalizedDataAdapter(DbDataReader dataReader) : base(dataReader)
        {
        }

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
        string ILocalized.Culture => CultureInfo.Name;
        [Column(name: GeneralEntityDefaults.FieldLangCode)]
        string? ILocalizable.Culture => CultureInfo.Name;

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

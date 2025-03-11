using System.Globalization;
using CraftedSolutions.MarBasSchema;

namespace CraftedSolutions.MarBasSchema.Sys
{
    public class SystemLanguage : ISystemLanguage
    {
        protected readonly UpdateableTracker _fieldTracker;

        protected string _isoCode;
        protected string _label;
        protected string? _labelNative;

        public SystemLanguage(string isoCode, string label, string? labelNative = null)
        {
            _fieldTracker = new UpdateableTracker();
            _isoCode = isoCode;
            _label = label;
            _labelNative = labelNative;
        }

        public SystemLanguage(CultureInfo cultureInfo) :
            this(cultureInfo.IetfLanguageTag, cultureInfo.EnglishName, cultureInfo.NativeName)
        {
        }

        public SystemLanguage(ISystemLanguage other)
        {
            _fieldTracker = other.FieldTracker;
            _isoCode = other.IsoCode;
            _label = other.Label;
            _labelNative = other.LabelNative;
        }

        public string IsoCode
        {
            get => _isoCode;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_isoCode, value))
                {
                    _isoCode = value;
                    _fieldTracker.TrackPropertyChange<ISystemLanguage>();
                }
            }
        }

        public string Label
        {
            get => _label;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_label, value))
                {
                    _label = value;
                    _fieldTracker.TrackPropertyChange<ISystemLanguage>();
                }
            }
        }

        public string? LabelNative
        {
            get => _labelNative;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_labelNative, value))
                {
                    _labelNative = value;
                    _fieldTracker.TrackPropertyChange<ISystemLanguage>();
                }
            }
        }

        public UpdateableTracker FieldTracker => _fieldTracker;

        public ISet<string> GetDirtyFields<TScope>() => _fieldTracker.GetScope<TScope>();

        public CultureInfo ToCultureInfo() => string.IsNullOrEmpty(IsoCode) ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(IsoCode);

        public static implicit operator SystemLanguage(CultureInfo cultureInfo) => new(cultureInfo);
        public static implicit operator CultureInfo(SystemLanguage lang) => lang.ToCultureInfo();
        public static implicit operator string(SystemLanguage lang) => lang.IsoCode;
    }
}

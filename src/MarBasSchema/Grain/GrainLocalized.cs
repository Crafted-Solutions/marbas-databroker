using CraftedSolutions.MarBasCommon;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text.Json.Serialization;

namespace CraftedSolutions.MarBasSchema.Grain
{
    public class GrainLocalized : GrainExtended, IGrainLocalized
    {
        protected CultureInfo _culture;
        protected string? _label;

        public GrainLocalized(string? name = null, IIdentifiable? parent = null, IPrincipal? creator = null, CultureInfo? culture = null)
            : base(name, parent, creator)
        {
            _culture = culture ?? SchemaDefaults.Culture;
            _fieldTracker.AddScope<IGrainLocalized>();
        }

        public GrainLocalized(IGrain other)
            : base(other)
        {
            if (other is ILocalized localized)
            {
                _culture = localized.CultureInfo;
            }
            if (other is ILabeled labeled)
            {
                _label = labeled.Label;
            }
            if (null == _culture)
            {
                _culture = SchemaDefaults.Culture;
            }
            _fieldTracker.AddScope<IGrainLocalized>();
        }

        [JsonIgnore]
        [IgnoreDataMember]
        public CultureInfo CultureInfo => _culture;
        public string Culture => _culture.Name;

        public string? Label
        {
            get => string.IsNullOrEmpty(_label) ? Name : _label;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_label, value))
                {
                    _label = value;
                    _fieldTracker.TrackPropertyChange<IGrainLocalized>();
                }
            }
        }
    }
}

using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text.Json.Serialization;
using MarBasCommon;
using MarBasSchema.Access;

namespace MarBasSchema.Grain
{
    public class GrainLocalized : GrainExtended, IGrainLocalized
    {
        protected CultureInfo _culture;
        protected string? _label;

        protected GrainLocalized(Guid id, string? name = null, IIdentifiable? parent = null, IPrincipal? creator = null, CultureInfo? culture = null)
            : this(name, parent, creator, culture)
        {
            _props.Id = id;
        }

        public GrainLocalized(string? name = null, IIdentifiable? parent = null, IPrincipal? creator = null, CultureInfo? culture = null)
            : base(name, parent, creator)
        {
            _culture = culture ?? SchemaDefaults.Culture;
            _fieldTracker.AddScope<IGrainLocalized>();
            _permissions = GrainAccessFlag.Read;
        }

        public GrainLocalized(IGrainBase other)
            : base(other)
        {
            if (other is IGrainLocalized localized)
            {
                _culture = localized.CultureInfo ?? SchemaDefaults.Culture;
                _label = localized.Label;
            }
            else
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

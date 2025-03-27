using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon;

namespace CraftedSolutions.MarBasSchema.Grain
{
    public class GrainLabel : IGrainLabel, IUpdateable
    {
        protected readonly UpdateableTracker _fieldTracker;
        protected IIdentifiable _grain;
        protected CultureInfo _culture;
        protected string? _label;

        public GrainLabel(string label, IIdentifiable grain, CultureInfo? culture = null)
        {
            _fieldTracker = new UpdateableTracker();
            _label = label;
            _grain = grain;
            _culture = culture ?? SchemaDefaults.Culture;
        }

        public GrainLabel(IGrainLabel other)
        {
            _fieldTracker = other is IUpdateable updateable ? updateable.FieldTracker : new UpdateableTracker();

            _label = other.Label;
            _grain = other.Grain;
            _culture = other.CultureInfo;
        }

        public string? Label
        {
            get => _label;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_label, value))
                {
                    _label = value;
                    _fieldTracker.TrackPropertyChange<IGrainLabel>();
                }
            }
        }
        [ReadOnly(true)]
        [JsonIgnore]
        [IgnoreDataMember]
        public IIdentifiable Grain
        {
            get => _grain;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_grain, value))
                {
                    _grain = value;
                    _fieldTracker.TrackPropertyChange<IGrainLabel>();
                }
            }
        }

        public Guid GrainId => _grain.Id;

        [JsonIgnore]
        [IgnoreDataMember]
        public CultureInfo CultureInfo
        {
            get => _culture;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_culture, value))
                {
                    _culture = value;
                    _fieldTracker.TrackPropertyChange<IGrainLabel>();
                }
            }
        }
        public string Culture => _culture.IetfLanguageTag;

        public UpdateableTracker FieldTracker => _fieldTracker;

        public ISet<string> GetDirtyFields<TScope>() => _fieldTracker.GetScope<TScope>();
    }
}

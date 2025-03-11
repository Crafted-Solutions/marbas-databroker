using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema;

namespace CraftedSolutions.MarBasSchema.Grain
{
    public abstract class TraitBase : Identifiable, ITraitBase
    {
        protected readonly UpdateableTracker _fieldTracker;

        protected TraitRef _traitRef;
        protected int _ord;

        public TraitBase(IIdentifiable grain, IIdentifiable propdef, CultureInfo? culture = null)
            : base(Guid.NewGuid())
        {
            _fieldTracker = new UpdateableTracker();
            _traitRef = new(grain, propdef, culture);
            _ord = 0;
        }

        public TraitBase(ITraitBase other)
            : base(other.Id)
        {
            _fieldTracker = other.FieldTracker ?? new UpdateableTracker();
            _traitRef = new(other.Grain, other.PropDef, other.CultureInfo)
            {
                Revision = other.Revision
            };
            _ord = other.Ord;
        }

        [JsonIgnore]
        [IgnoreDataMember]
        public IIdentifiable Grain
        {
            get => _traitRef.Grain;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_traitRef.Grain, value))
                {
                    _traitRef.Grain = value;
                    _fieldTracker.TrackPropertyChange<ITraitBase>();
                }
            }
        }
        public Guid GrainId => _traitRef.GrainId;

        [JsonIgnore]
        [IgnoreDataMember]
        public IIdentifiable PropDef
        {
            get => _traitRef.PropDef;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_traitRef.PropDef, value))
                {
                    _traitRef.PropDef = value;
                    _fieldTracker.TrackPropertyChange<ITraitBase>();
                }
            }
        }
        public Guid PropDefId => _traitRef.PropDefId;

        [JsonIgnore]
        [IgnoreDataMember]
        public CultureInfo? CultureInfo
        {
            get => _traitRef.CultureInfo;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_traitRef.CultureInfo, value))
                {
                    _traitRef.CultureInfo = value;
                    _fieldTracker.TrackPropertyChange<ITraitBase>();
                }
            }
        }
        public string? Culture => _traitRef.Culture;

        public int Ord
        {
            get => _ord;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_ord, value))
                {
                    _ord = value;
                    _fieldTracker.TrackPropertyChange<ITraitBase>();
                }
            }
        }

        public int Revision
        {
            get => _traitRef.Revision;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_traitRef.Revision, value))
                {
                    _traitRef.Revision = value;
                    _fieldTracker.TrackPropertyChange<ITraitBase>();
                }
            }
        }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual bool IsNull => null == Value;

        public virtual TraitValueType ValueType => PropDef is IValueTypeConstraint typeConstraint ? typeConstraint.ValueType : TraitValueType.Text;
        public abstract object? Value { get; }

        [JsonIgnore]
        [IgnoreDataMember]
        public UpdateableTracker FieldTracker => _fieldTracker;

        public ISet<string> GetDirtyFields<TScope>() => _fieldTracker.GetScope<TScope>();
    }
}

using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasSchema.GrainDef
{
    public class GrainPropDef : GrainLocalized, IGrainPropDefLocalized
    {
        protected static readonly IGrain DefaultType = new GrainPlain()
        {
            Id = SchemaDefaults.PropDefTypeDefID,
            Name = SchemaDefaults.PropDefTypeName,
        };

        protected TraitValueType _valueType;
        protected IIdentifiable? _valueConstraint;
        protected string? _constraintParams;
        protected int[] _cardinality;
        protected bool _versionable;
        protected bool _localizable;

        // TODO remove this constructor, GrainTransportBroker should use its own subclass
        [Obsolete("Do not use, declare own subclass to initalize Id")]
        public GrainPropDef(Guid id, string? name = null, IIdentifiable? parent = null, IPrincipal? creator = null, CultureInfo? culture = null)
            : this(name, parent, creator, culture)
        {
            _props.Id = id;
        }

        public GrainPropDef(string? name = null, IIdentifiable? parent = null, IPrincipal? creator = null, CultureInfo? culture = null)
            : base(name, parent, creator, culture)
        {
            _fieldTracker.AddScope<IGrainPropDef>();
            _typeConstraint = new SimpleTypeConstraint(DefaultType);
            _cardinality = [1, 1];
            _versionable = true;
            _localizable = true;
        }

        public GrainPropDef(IGrain other)
            : base(other)
        {
            _fieldTracker.AddScope<IGrainPropDef>();
            if (null == _typeConstraint)
            {
                _typeConstraint = new SimpleTypeConstraint(DefaultType);
            }
            if (other is IPropDef propDef)
            {
                _valueConstraint = other is IGrainPropDef grainPropDef ? grainPropDef.ValueConstraint : (Identifiable?)propDef.ValueConstraintId;
                _constraintParams = propDef.ConstraintParams;
                _cardinality = [propDef.CardinalityMin, propDef.CardinalityMax];
                _versionable = propDef.Versionable;
                _localizable = propDef.Localizable;
            }
            else
            {
                _cardinality = [1, 1];
                _versionable = true;
                _localizable = true;
            }
            if (other is IValueTypeConstraint valueType)
            {
                _valueType = valueType.ValueType;
            }
        }

        public TraitValueType ValueType
        {
            get => _valueType;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_valueType, value))
                {
                    _valueType = value;
                    _fieldTracker.TrackPropertyChange<IGrainPropDef>();
                }
            }
        }

        [JsonIgnore]
        [IgnoreDataMember]
        public IIdentifiable? ValueConstraint
        {
            get => _valueConstraint;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_valueConstraint, value))
                {
                    _valueConstraint = value;
                    _fieldTracker.TrackPropertyChange<IGrainPropDef>();
                }
            }
        }
        public Guid? ValueConstraintId
        {
            get => ValueConstraint?.Id;
            internal set => ValueConstraint = (Identifiable?)value;
        }
        Guid? IPropDef.ValueConstraintId { get => ValueConstraintId; set => ValueConstraintId = value; }

        public string? ConstraintParams
        {
            get => _constraintParams;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_constraintParams, value))
                {
                    _constraintParams = value;
                    _fieldTracker.TrackPropertyChange<IGrainPropDef>();
                }
            }
        }

        public int CardinalityMin
        {
            get => _cardinality[0];
            set
            {
                if (_fieldTracker.IsChangeAccepted(_cardinality[0], value))
                {
                    _cardinality[0] = value;
                    _fieldTracker.TrackPropertyChange<IGrainPropDef>();
                }
            }
        }

        public int CardinalityMax
        {
            get => _cardinality[1];
            set
            {
                if (_fieldTracker.IsChangeAccepted(_cardinality[1], value))
                {
                    _cardinality[1] = value;
                    _fieldTracker.TrackPropertyChange<IGrainPropDef>();
                }
            }
        }

        public bool Versionable
        {
            get => _versionable;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_versionable, value))
                {
                    _versionable = value;
                    _fieldTracker.TrackPropertyChange<IGrainPropDef>();
                }
            }

        }

        public bool Localizable
        {
            get => _localizable;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_localizable, value))
                {
                    _localizable = value;
                    _fieldTracker.TrackPropertyChange<IGrainPropDef>();
                }
            }
        }
    }
}

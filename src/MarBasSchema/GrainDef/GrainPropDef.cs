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
        protected static readonly IGrainBase DefaultType = new GrainBase(SchemaDefaults.PropDefTypeDefID, SchemaDefaults.PropDefTypeName);

        protected TraitValueType _valueType;
        protected IIdentifiable? _valueConstraint;
        protected string? _constraintParams;
        protected int[] _cardinality;
        protected bool _versionable;
        protected bool _localizable;

        public GrainPropDef(Guid id, string? name = null, IIdentifiable? parent = null, IPrincipal? creator = null, CultureInfo? culture = null)
            : this(name, parent, creator, culture)
        {
            _props.Id = id;
        }

        public GrainPropDef(string? name = null, IIdentifiable? parent = null, IPrincipal? creator = null, CultureInfo? culture = null)
            : base(name, parent, creator, culture)
        {
            _fieldTracker.AddScope<IGrainPropDef>();
            _cardinality = new int[] { 1, 1 };
            _versionable = true;
            _localizable = true;
        }

        public GrainPropDef(IGrainBase other)
            : base(other)
        {
            _fieldTracker.AddScope<IGrainPropDef>();
            if (other is IGrainPropDef propDef)
            {
                _valueConstraint = propDef.ValueConstraint;
                _constraintParams = propDef.ConstraintParams;
                _cardinality = new int[] { propDef.CardinalityMin, propDef.CardinalityMax };
                _versionable = propDef.Versionable;
                _localizable = propDef.Localizable;
            }
            else
            {
                _cardinality = new int[] { 1, 1 };
                _versionable = true;
                _localizable = true;
            }
            if (other is IValueTypeConstraint valueType)
            {
                _valueType = valueType.ValueType;
            }
        }

        public override string? TypeName => base.TypeName ?? SchemaDefaults.PropDefTypeName;

        [JsonIgnore]
        [IgnoreDataMember]
        public override IIdentifiable? TypeDef
        {
            get => base.TypeDef as IGrainBase ?? DefaultType;
            set => base.TypeDef = value ?? DefaultType;
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

using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasSchema.GrainDef
{
    public class GrainTypeDef : GrainLocalized, IGrainTypeDefLocalized
    {
        protected string? _impl;
        protected ISet<IIdentifiable> _mixins;
        protected IIdentifiable? _defaultInst;

        // TODO remove this constructor, GrainTransportBroker should use its own subclass
        [Obsolete("Do not use, declare own subclass to initalize Id")]
        public GrainTypeDef(Guid id, string? name, IIdentifiable? parent, IEnumerable<IIdentifiable>? mixins = null, IPrincipal? creator = null, CultureInfo? culture = null)
            : this(name, parent, mixins, creator, culture)
        {
            _props.Id = id;
        }

        public GrainTypeDef(string? name = null, IIdentifiable? parent = null, IEnumerable<IIdentifiable>? mixins = null, IPrincipal? creator = null, CultureInfo? culture = null)
            : base(name, parent, creator, culture)
        {
            _mixins = null == mixins ? [] : new HashSet<IIdentifiable>(mixins);
            _fieldTracker.AddScope<IGrainTypeDef>();
        }

        public GrainTypeDef(IGrain other)
            : base(other)
        {
            if (other is ITypeDef typeDef)
            {
                _impl = typeDef.Impl;
                if (other is IGrainTypeDef grainTypeDef)
                {
                    _mixins = grainTypeDef.MixIns.ToHashSet();
                    _defaultInst = grainTypeDef.DefaultInstance;
                }
                else
                {
                    _mixins = typeDef.MixInIds.Select(x => (IIdentifiable)(Identifiable)x).ToHashSet();
                }
            }
            else
            {
                _mixins = new HashSet<IIdentifiable>();
            }
            _typeConstraint = null;
            _fieldTracker.AddScope<IGrainTypeDef>();
        }

        public string? Impl
        {
            get => _impl;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_impl, value))
                {
                    _impl = value;
                    _fieldTracker.TrackPropertyChange<IGrainTypeDef>();
                }
            }
        }

        public Guid? DefaultInstanceId => _defaultInst?.Id;

        [JsonIgnore]
        [IgnoreDataMember]
        public IIdentifiable? DefaultInstance
        {
            get => _defaultInst;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_defaultInst, value))
                {
                    _defaultInst = value;
                    _fieldTracker.TrackPropertyChange<IGrainTypeDef>();
                }
            }
        }

        public void AddMixIn(IIdentifiable typeDef)
        {
            if (_mixins.Add(typeDef))
            {
                _fieldTracker.TrackPropertyChange<IGrainTypeDef>(nameof(MixIns));
            }
        }

        public void RemoveMixIn(IIdentifiable typeDef)
        {
            if (_mixins.Remove(typeDef))
            {
                _fieldTracker.TrackPropertyChange<IGrainTypeDef>(nameof(MixIns));
            }
        }

        public void ClearMixIns()
        {
            if (0 < _mixins.Count)
            {
                _mixins.Clear();
                _fieldTracker.TrackPropertyChange<IGrainTypeDef>(nameof(MixIns));
            }
        }

        public void ReplaceMixIns(IEnumerable<IIdentifiable>? mixins)
        {
            if (null == mixins)
            {
                ClearMixIns();
            }
            else
            {
                _mixins = new HashSet<IIdentifiable>(mixins);
                _fieldTracker.TrackPropertyChange<IGrainTypeDef>(nameof(MixIns));
            }
        }

        [JsonIgnore]
        [IgnoreDataMember]
        public IEnumerable<IIdentifiable> MixIns => _mixins;
        public IEnumerable<Guid> MixInIds => _mixins.Select(x => x.Id);

        [JsonIgnore]
        [IgnoreDataMember]
        public override IIdentifiable? TypeDef
        {
            get => base.TypeDef;
            set
            {
                if (null != value)
                {
                    throw new NotSupportedException($"{nameof(TypeDef)} should always be null");
                }
            }
        }
    }
}

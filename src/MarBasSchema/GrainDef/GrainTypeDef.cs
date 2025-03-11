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
        protected static readonly IGrainBase DefaultType = new GrainBase(SchemaDefaults.TypeDefTypeDefID, SchemaDefaults.TypeDefTypeName);

        protected string? _impl;
        protected ISet<IIdentifiable> _mixins;
        protected IIdentifiable? _defaultInst;

        public GrainTypeDef(Guid id, string? name, IIdentifiable? parent, IEnumerable<IIdentifiable>? mixins = null, IPrincipal? creator = null, CultureInfo? culture = null)
            : this(name, parent, mixins, creator, culture)
        {
            _props.Id = id;
        }

        public GrainTypeDef(string? name, IIdentifiable? parent, IEnumerable<IIdentifiable>? mixins = null, IPrincipal? creator = null, CultureInfo? culture = null)
            : base(name, parent, creator, culture)
        {
            _mixins = null == mixins ? new HashSet<IIdentifiable>() : new HashSet<IIdentifiable>(mixins);
            _fieldTracker.AddScope<IGrainTypeDef>();
        }

        public GrainTypeDef(IGrainBase other)
            : base(other)
        {
            if (other is IGrainTypeDef typeDef)
            {
                _impl = typeDef.Impl;
                _mixins = typeDef.MixIns.ToHashSet();
                _defaultInst = typeDef.DefaultInstance;
            }
            else
            {
                _mixins = new HashSet<IIdentifiable>();
            }
            _fieldTracker.AddScope<IGrainTypeDef>();
        }

        public string? Impl
        {
            get => _impl;
            set
            {
                if (_impl != value)
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
                if (_defaultInst != value)
                {
                    _fieldTracker.TrackPropertyChange<IGrainTypeDef>();
                    _defaultInst = value;
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
                _mixins.Clear();
            }
            else
            {
                _mixins = new HashSet<IIdentifiable>(mixins);
            }
            _fieldTracker.TrackPropertyChange<IGrainTypeDef>(nameof(MixIns));
        }

        [JsonIgnore]
        [IgnoreDataMember]
        public IEnumerable<IIdentifiable> MixIns => _mixins;
        public IEnumerable<Guid> MixInIds => _mixins.Select(x => x.Id);

        [JsonIgnore]
        [IgnoreDataMember]
        public override IIdentifiable? TypeDef
        {
            get => base.TypeDef as IGrainBase;
            set => base.TypeDef = value as IGrainBase;
        }
    }
}

using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text.Json.Serialization;
using MarBasCommon;

namespace MarBasSchema.Grain
{
    public class GrainBase : IGrainBase, ICloneable
    {
        protected readonly UpdateableTracker _fieldTracker;
        protected GrainPlain _props = new();

        protected IIdentifiable? _parent;
        protected ITypeConstraint? _typeConstraint;

        internal GrainBase(Guid id, string? name = null, IIdentifiable? parent = null, IPrincipal? creator = null)
            : this(name, parent, creator)
        {
            _props.Id = id;
        }

        public GrainBase(string? name = null, IIdentifiable? parent = null, IPrincipal? creator = null)
        {
            _fieldTracker = new UpdateableTracker();
            _parent = parent;

            _props.Name = string.IsNullOrEmpty(name) ? $"Unnamed_{_props.Id:D}" : name;
            _props.MTime = DateTime.Now;
            _props.CTime = DateTime.Now;
            _props.Owner = creator?.Identity?.Name ?? SchemaDefaults.SystemUserName;
            _props.ParentId = _parent?.Id;
        }

        public GrainBase(IGrainBase other, IGrainBase? extension = null)
        {
            _fieldTracker = other.FieldTracker ?? new UpdateableTracker();
            _parent = other.Parent;
            _typeConstraint = new SimpleTypeConstraint(null == other.TypeName && null != extension ? extension : other);

            _props = new(other);
            SyncPath();
        }

        public Guid Id => _props.Id;

        public Guid? ParentId { get => _parent?.Id; }
        [JsonIgnore]
        [IgnoreDataMember]
        public IIdentifiable? Parent
        {
            get => _parent;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_parent, value))
                {
                    _parent = value;
                    _fieldTracker.TrackPropertyChange<IGrainBase>();
                }
            }
        }

        public Guid? TypeDefId { get => TypeDef?.Id; }
        [JsonIgnore]
        [IgnoreDataMember]
        public virtual IIdentifiable? TypeDef
        {
            get => _typeConstraint?.TypeDef;
            set
            {
                if (null == value)
                {
                    _typeConstraint = null;
                }
                else if (null == _typeConstraint && null != value)
                {
                    if (value is ITypeConstraint typeConstraint)
                    {
                        _typeConstraint = typeConstraint;
                    }
                    else if (value is INamed named)
                    {
                        _typeConstraint = SimpleTypeConstraint.CreateFrom((INamedIdentifiable)named);
                    }
                    else
                    {
                        _typeConstraint = new SimpleTypeConstraint(value.Id);
                    }
                }
                _fieldTracker.TrackPropertyChange<IGrainBase>();
            }
        }

        public string Name
        {
            get => _props.Name ?? $"Unnamed_{Id:D}";
            set
            {
                var newName = null == value ? null : SanitizeName(value);
                if (_fieldTracker.IsChangeAccepted(_props.Name, newName))
                {
                    _props.Name = newName!;
                    _fieldTracker.TrackPropertyChange<IGrainBase>();
                    SyncPath();
                }
            }
        }

        public DateTime CTime
        {
            get => _props.CTime;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_props.CTime, value))
                {
                    _props.CTime = value;
                    _fieldTracker.TrackPropertyChange<IGrainBase>();
                }
            }
        }

        public DateTime MTime
        {
            get => _props.MTime;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_props.MTime, value))
                {
                    _props.MTime = value;
                    _fieldTracker.TrackPropertyChange<IGrainBase>();
                }
            }
        }

        public virtual string? TypeName => _typeConstraint?.TypeName;

        public string Owner
        {
            get => _props.Owner;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_props.Owner, value))
                {
                    _props.Owner = value;
                    _fieldTracker.TrackPropertyChange<IGrainBase>();
                }
            }
        }

        public int Revision
        {
            get => _props.Revision;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_props.Revision, value))
                {
                    _props.Revision = value;
                    _fieldTracker.TrackPropertyChange<IGrainBase>();
                }
            }
        }

        public string? SortKey
        {
            get => _props.SortKey;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_props.SortKey, value))
                {
                    _props.SortKey = value;
                    _fieldTracker.TrackPropertyChange<IGrainBase>();
                }
            }
        }
        public int CustomFlag
        {
            get => _props.CustomFlag;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_props.CustomFlag, value))
                {
                    _props.CustomFlag = value;
                    _fieldTracker.TrackPropertyChange<IGrainBase>();
                }
            }
        }

        public string? XAttrs
        {
            get => _props.XAttrs;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_props.XAttrs, value))
                {
                    _props.XAttrs = value;
                    _fieldTracker.TrackPropertyChange<IGrainBase>();
                }
            }
        }

        public string? Path => _props.Path;

        [JsonIgnore]
        [IgnoreDataMember]
        public UpdateableTracker FieldTracker => _fieldTracker;

        public ISet<string> GetDirtyFields<T>() => _fieldTracker.GetScope<T>();


        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public static string SanitizeName(string name)
        {
            var result = name.Normalize();
            if (255 < result.Length)
            {
                result = $"{result.Remove(252)}...";
            }
            return System.IO.Path.GetInvalidFileNameChars().Aggregate(result, (current, c) =>
            {
                return current.Replace(c, '!');
            });
        }

        protected void SyncPath()
        {
            var name = Name;
            if (!string.IsNullOrEmpty(_props.Path) && !_props.Path.EndsWith(name, StringComparison.InvariantCulture))
            {
                _props.Path = _props.Path.Remove(_props.Path.LastIndexOf("/") + 1);
                _props.Path += name;
            }
        }

        protected interface INamedIdentifiable: IIdentifiable, INamed { }
    }
}

using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasCommon.Reflection;

namespace CraftedSolutions.MarBasSchema.Grain
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

            _props.Name = string.IsNullOrEmpty(name) ? MakeEmptyName(_props.Id) : name;
            _props.MTime = _props.CTime = DateTime.Now;
            _props.Owner = creator?.Identity?.Name ?? SchemaDefaults.SystemUserName;
            _props.ParentId = _parent?.Id;
        }

        public GrainBase(IGrain other, ITypeConstraint? typeExtension = null)
        {
            _fieldTracker = other is IUpdateable updateable ? updateable.FieldTracker.MakeClone()! : new UpdateableTracker();
            _parent = other is IGrainBase otherBase ? otherBase.Parent : (Identifiable?) other.ParentId;

            if (other is ITypeConstraint otherTyped && null != otherTyped.TypeDefId)
            {
                _typeConstraint = new SimpleTypeConstraint(otherTyped);
            }
            if ((null == _typeConstraint || null == _typeConstraint.TypeName) && null != typeExtension)
            {
                _typeConstraint = new SimpleTypeConstraint(typeExtension);
            }
            if (null == _typeConstraint && null != other.TypeDefId)
            {
                _typeConstraint = new SimpleTypeConstraint((Guid)other.TypeDefId);
            }

            _props = new(other);
            SyncPath();
        }

        public Guid Id => _props.Id;

        public Guid? ParentId => _parent?.Id;
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

        public Guid? TypeDefId => _typeConstraint?.TypeDefId;
        public virtual string? TypeName => _typeConstraint?.TypeName;
        [JsonIgnore]
        [IgnoreDataMember]
        public virtual IIdentifiable? TypeDef
        {
            get => _typeConstraint?.TypeDef;
            set
            {
                var changed = false;
                if (null == value)
                {
                    if (_fieldTracker.AcceptAllChanges || null != _typeConstraint)
                    {
                        _typeConstraint = null;
                        changed = true;
                    }
                }
                // TODO since IGrainBase itself is ITypeConstraint AND IIdentifiable using this clause would only produce confusion
                //else if (value is ITypeConstraint typeConstraint)
                //{
                //    if (_fieldTracker.AcceptAllChanges || _typeConstraint != typeConstraint)
                //    {
                //        _typeConstraint = typeConstraint;
                //        changed = true;
                //    }
                //}
                else if (_fieldTracker.AcceptAllChanges || value.Id != _typeConstraint?.TypeDefId)
                {
                    if (value is not IGrain && value is INamed named)
                    {
                        _typeConstraint = new SimpleTypeConstraint(value.Id, named.Name);
                    }
                    else
                    {
                        _typeConstraint = new SimpleTypeConstraint(value);
                    }
                    changed = true;
                }
                if (changed)
                {
                    _fieldTracker.TrackPropertyChange<IGrainBase>();
                }
            }
        }

        public string Name
        {
            get => string.IsNullOrEmpty(_props.Name) ? MakeEmptyName(Id) : _props.Name;
            set
            {
                var newName = string.IsNullOrEmpty(value) ? MakeEmptyName(Id) : SanitizeName(value);
                if (_fieldTracker.IsChangeAccepted(_props.Name, newName))
                {
                    _props.Name = newName;
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
            return MemberwiseClone();
        }

        protected void SyncPath()
        {
            var name = Name;
            if (!string.IsNullOrEmpty(_props.Path) && !_props.Path.EndsWith(name, StringComparison.InvariantCulture))
            {
                _props.Path = _props.Path.Remove(_props.Path.LastIndexOf('/') + 1);
                _props.Path += name;
            }
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

        public static string MakeEmptyName(Guid id) => $"Unnamed_{id:D}";
    }
}

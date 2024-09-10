using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MarBasCommon;

namespace MarBasSchema.Access
{
    public class SchemaAclEntry : ISchemaAclEntry
    {
        protected readonly UpdateableTracker _fieldTracker;
        protected IIdentifiable _role;
        protected IIdentifiable _grain;
        protected GrainAccessFlag _permissions;
        protected GrainAccessFlag _restrictions;
        protected bool _inherit;
        protected IIdentifiable? _sourceGrain;

        public SchemaAclEntry(IIdentifiable role, IIdentifiable grain, GrainAccessFlag permissions = GrainAccessFlag.Read, GrainAccessFlag restrictions = GrainAccessFlag.None, bool inherit = false)
        {
            _fieldTracker = new UpdateableTracker();
            _role = role;
            _grain = grain;
            _permissions = permissions;
            _restrictions = restrictions;
            _inherit = inherit;
        }

        public SchemaAclEntry(ISchemaAclEntry other)
        {
            _fieldTracker = other.FieldTracker ?? new UpdateableTracker();
            _role = other.Role;
            _grain = other.Grain;
            _permissions = other.PermissionMask;
            _restrictions = other.RestrictionMask;
            _inherit = other.Inherit;
            _sourceGrain = other.SourceGrain;
        }

        public Guid RoleId => _role.Id;
        [JsonIgnore]
        [IgnoreDataMember]
        public IIdentifiable Role
        {
            get => _role;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_role, value))
                {
                    _role = value;
                    _fieldTracker.TrackPropertyChange<ISchemaAclEntry>();
                }
            }
        }

        public Guid GrainId => _grain.Id;
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
                    _fieldTracker.TrackPropertyChange<ISchemaAclEntry>();
                }
            }
        }

        public bool Inherit
        {
            get => _inherit;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_inherit, value))
                {
                    _inherit = value;
                    _fieldTracker.TrackPropertyChange<ISchemaAclEntry>();
                }
            }
        }

        public GrainAccessFlag PermissionMask
        {
            get => _permissions;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_permissions, value))
                {
                    _permissions = value;
                    _fieldTracker.TrackPropertyChange<ISchemaAclEntry>();
                }
            }
        }

        public GrainAccessFlag RestrictionMask
        {
            get => _restrictions;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_restrictions, value))
                {
                    _restrictions = value;
                    _fieldTracker.TrackPropertyChange<ISchemaAclEntry>();
                }
            }
        }

        public Guid? SourceGrainId => _sourceGrain?.Id;

        [JsonIgnore]
        [IgnoreDataMember]
        public IIdentifiable? SourceGrain => _sourceGrain;

        [JsonIgnore]
        [IgnoreDataMember]
        public UpdateableTracker FieldTracker => _fieldTracker;

        public ISet<string> GetDirtyFields<T>() => _fieldTracker.GetScope<T>();
    }
}

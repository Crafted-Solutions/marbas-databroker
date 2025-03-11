using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon;

namespace CraftedSolutions.MarBasSchema.Access
{
    public class SchemaRole : Identifiable, ISchemaRole
    {
        public static readonly ISchemaRole Everyone = new SchemaRole(SchemaDefaults.EveryoneRoleID, "Everyone");
        public static readonly ISchemaRole Superuser = new SchemaRole(SchemaDefaults.SuperuserRoleID, "Superuser", RoleEntitlement.Full);

        protected readonly UpdateableTracker _fieldTracker;
        protected string _name;
        protected RoleEntitlement _entitlement;

        protected SchemaRole(Guid id, string? name = null, RoleEntitlement entitlement = RoleEntitlement.None)
            : this(name, entitlement)
        {
            _id = id;
        }

        public SchemaRole(string? name = null, RoleEntitlement entitlement = RoleEntitlement.None)
            : base(Guid.NewGuid())
        {
            _fieldTracker = new UpdateableTracker();
            _name = name!;
            _entitlement = entitlement;
        }

        public SchemaRole(ISchemaRole other)
            : base(other)
        {
            _fieldTracker = other.FieldTracker ?? new UpdateableTracker();
            _name = other.Name;
            _entitlement = other.Entitlement;
        }

        public RoleEntitlement Entitlement
        {
            get => _entitlement;
            set
            {
                if (_fieldTracker.IsChangeAccepted(value, _entitlement))
                {
                    _entitlement = value;
                    _fieldTracker.TrackPropertyChange<ISchemaRole>();
                }
            }
        }

        public string Name
        {
            get => _name ?? $"Role{_id:D}";
            set
            {
                if (_fieldTracker.IsChangeAccepted(_name, value))
                {
                    _name = value;
                    _fieldTracker.TrackPropertyChange<ISchemaRole>();
                }
            }
        }

        [JsonIgnore]
        [IgnoreDataMember]
        public UpdateableTracker FieldTracker => _fieldTracker;

        public ISet<string> GetDirtyFields<T>() => _fieldTracker.GetScope<T>();
    }
}

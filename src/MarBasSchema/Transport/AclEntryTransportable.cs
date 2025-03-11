using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Access;

namespace CraftedSolutions.MarBasSchema.Transport
{
    public class AclEntryTransportable : IAclEntryTransportable
    {
        private Guid _grainId;

        [JsonConstructor]
        public AclEntryTransportable()
        {
        }

        public AclEntryTransportable(IAclEntry other)
        {
            _grainId = other.GrainId;
            RoleId = other.RoleId;
            Inherit = other.Inherit;
            PermissionMask = other.PermissionMask;
            RestrictionMask = other.RestrictionMask;
        }

        [JsonIgnore]
        [IgnoreDataMember]
        public Guid GrainId => _grainId;
        [JsonIgnore]
        [IgnoreDataMember]
        public IIdentifiable Grain { get => (Identifiable)_grainId; set => _grainId = value.Id; }

        public bool Inherit { get; set; }
        public GrainAccessFlag PermissionMask { get; set; }
        public GrainAccessFlag RestrictionMask { get; set; }

        public Guid RoleId { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public IIdentifiable Role { get => (Identifiable)RoleId; set => RoleId = value.Id; }
    }
}

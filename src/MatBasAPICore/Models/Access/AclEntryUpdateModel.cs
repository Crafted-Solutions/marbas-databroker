using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MarBasCommon;
using MarBasSchema.Access;

namespace MarBasAPICore.Models.Access
{
    public sealed class AclEntryUpdateModel : IAclEntryUpdateModel
    {
        private readonly ISchemaAclEntry _entry = new AclWrapper();

        [Required]
        public Guid RoleId { get => _entry.RoleId; set => _entry.Role = (Identifiable)value; }
        public Guid GrainId { get => _entry.GrainId; set => _entry.Grain = (Identifiable)value; }
        public bool? Inherit { get => _entry.Inherit; set { if (null != value) _entry.Inherit = (bool)value; } }
        public GrainAccessFlag? PermissionMask { get => _entry.PermissionMask; set { if (null != value) _entry.PermissionMask = (GrainAccessFlag)value; } }
        public GrainAccessFlag? RestrictionMask { get => _entry.RestrictionMask; set { if (null != value) _entry.RestrictionMask = (GrainAccessFlag)value; } }

        [JsonIgnore]
        [IgnoreDataMember]
        public ISchemaAclEntry Entry => _entry;

        private class AclWrapper : SchemaAclEntry
        {
            public AclWrapper()
                : base((Identifiable)Guid.Empty, (Identifiable)Guid.Empty)
            {
                _fieldTracker.AcceptAllChanges = true;
            }
        }

    }
}

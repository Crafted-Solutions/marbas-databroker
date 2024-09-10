using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MarBasSchema.Access;

namespace MarBasAPICore.Models.Access
{
    public sealed class RoleUpdateModel : IRoleUpdateModel
    {
        private RoleWrapper _role = new ();

        [Required]
        public Guid Id { get => _role.Id; set => _role.Id = value; }

        public string? Name { get => _role.Name; set => _role.Name = value ?? $"Role_{ Id.ToString("D")}"; }
        public RoleEntitlement? Capabilities { get => _role.Entitlement; set => _role.Entitlement = value ?? RoleEntitlement.None; }

        [JsonIgnore]
        [IgnoreDataMember]
        public ISchemaRole Role => _role;

        private class RoleWrapper: SchemaRole
        {
            public RoleWrapper()
            {
                _fieldTracker.AcceptAllChanges = true;
            }

            public new Guid Id { get => base.Id; set => _id = value; }
        }
    }
}

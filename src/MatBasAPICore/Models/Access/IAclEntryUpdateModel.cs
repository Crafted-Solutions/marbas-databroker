using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasSchema.Access;

namespace CraftedSolutions.MarBasAPICore.Models.Access
{
    public interface IAclEntryUpdateModel
    {
        [Required]
        Guid RoleId { get; set; }
        Guid GrainId { get; set; }
        bool? Inherit { get; set; }
        GrainAccessFlag? PermissionMask { get; set; }
        GrainAccessFlag? RestrictionMask { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        ISchemaAclEntry Entry { get; }
    }
}

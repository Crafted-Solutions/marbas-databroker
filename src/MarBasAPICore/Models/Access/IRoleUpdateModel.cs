using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasSchema.Access;

namespace CraftedSolutions.MarBasAPICore.Models.Access
{
    public interface IRoleUpdateModel
    {
        [Required]
        Guid Id { get; }
        string? Name { get; set; }
        RoleEntitlement? Capabilities { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        ISchemaRole Role { get; }
    }
}

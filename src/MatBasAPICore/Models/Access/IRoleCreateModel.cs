using System.ComponentModel.DataAnnotations;
using MarBasSchema.Access;

namespace MarBasAPICore.Models.Access
{
    public interface IRoleCreateModel
    {
        [Required]
        string Name { get; set; }
        RoleEntitlement? Entitlement { get; set; }
    }
}

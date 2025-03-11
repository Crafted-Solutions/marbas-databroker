using System.ComponentModel.DataAnnotations;
using CraftedSolutions.MarBasSchema.Access;

namespace CraftedSolutions.MarBasAPICore.Models.Access
{
    public interface IRoleCreateModel
    {
        [Required]
        string Name { get; set; }
        RoleEntitlement? Entitlement { get; set; }
    }
}

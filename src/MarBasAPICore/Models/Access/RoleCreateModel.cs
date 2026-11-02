using System.ComponentModel.DataAnnotations;
using CraftedSolutions.MarBasSchema.Access;

namespace CraftedSolutions.MarBasAPICore.Models.Access
{
    public sealed class RoleCreateModel : IRoleCreateModel
    {
        private RoleEntitlement _entitlement;
        private string? _name;

        [Required]
        public string Name { get => _name!; set => _name = value; }
        public RoleEntitlement? Entitlement { get => _entitlement; set => _entitlement = value ?? RoleEntitlement.None; }
    }
}

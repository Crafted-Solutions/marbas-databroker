using System.ComponentModel.DataAnnotations;
using CraftedSolutions.MarBasSchema.Access;

namespace CraftedSolutions.MarBasAPICore.Models.Access
{
    public sealed class AclEntryCreateModel : IAclEntryCreateModel
    {
        private bool _inherit = true;
        private GrainAccessFlag _permissions = GrainAccessFlag.Read;
        private GrainAccessFlag _restrictions = GrainAccessFlag.None;

        [Required]
        public Guid RoleId { get; set; }
        public Guid GrainId { get; set; }
        public bool? Inherit { get => _inherit; set => _inherit = value ?? true; }
        public GrainAccessFlag? PermissionMask { get => _permissions; set => _permissions = value ?? GrainAccessFlag.Read; }
        public GrainAccessFlag? RestrictionMask { get => _restrictions; set => _restrictions = value ?? GrainAccessFlag.None; }
    }
}

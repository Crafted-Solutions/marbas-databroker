﻿using System.ComponentModel.DataAnnotations;
using MarBasSchema.Access;

namespace MarBasAPICore.Models.Access
{
    public interface IAclEntryCreateModel
    {
        [Required]
        Guid RoleId { get; set; }
        Guid GrainId { get; set; }
        bool? Inherit { get; set; }
        GrainAccessFlag? PermissionMask { get; set; }
        GrainAccessFlag? RestrictionMask { get; set; }
    }
}

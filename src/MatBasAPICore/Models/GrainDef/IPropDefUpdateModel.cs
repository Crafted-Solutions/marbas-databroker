﻿using MarBasAPICore.Models.Grain;
using MarBasSchema;
using MarBasSchema.GrainDef;

namespace MarBasAPICore.Models.GrainDef
{
    public interface IPropDefUpdateModel : IGrainUpdateModel<IGrainPropDef>
    {
        TraitValueType? ValueType { get; set; }
        Guid? ValueConstraintId { get; set; }
        string? ConstraintParams { get; set; }
        int? CardinalityMin { get; set; }
        int? CardinalityMax { get; set; }
        bool? Versionable { get; set; }
        bool? Localizable { get; set; }
    }
}

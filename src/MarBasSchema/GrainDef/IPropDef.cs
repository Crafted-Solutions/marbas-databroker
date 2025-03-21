﻿using CraftedSolutions.MarBasSchema;

namespace CraftedSolutions.MarBasSchema.GrainDef
{
    public interface IPropDef : IValueTypeConstraint
    {
        Guid? ValueConstraintId { get; set; }
        string? ConstraintParams { get; set; }
        int CardinalityMin { get; set; }
        int CardinalityMax { get; set; }
        bool Versionable { get; set; }
        bool Localizable { get; set; }
    }
}

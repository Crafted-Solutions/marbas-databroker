namespace MarBasSchema.GrainDef
{
    public interface IPropDef: IValueTypeConstraint
    {
        Guid? ValueConstraintId { get; }
        string? ConstraintParams { get; set; }
        int CardinalityMin { get; set; }
        int CardinalityMax { get; set; }
        bool Versionable { get; set; }
        bool Localizable { get; set; }
    }
}

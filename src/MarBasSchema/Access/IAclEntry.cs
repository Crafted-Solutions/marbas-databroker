namespace CraftedSolutions.MarBasSchema.Access
{
    public interface IAclEntry : IAclEntryRef
    {
        bool Inherit { get; set; }
        GrainAccessFlag PermissionMask { get; set; }
        GrainAccessFlag RestrictionMask { get; set; }
    }
}

namespace CraftedSolutions.MarBasSchema.Broker
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class GrainQueryFilter : GrainBasicFilter, IGrainQueryFilter
    {
        public ITimeRangeConstraint? MTimeConstraint { get; set; }
    }
}

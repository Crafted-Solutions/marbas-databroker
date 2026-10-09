namespace CraftedSolutions.MarBasSchema
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class SimpleTimeRangeConstraint : ITimeRangeConstraint
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public RangeInclusionFlag Including { get; set; }
    }
}

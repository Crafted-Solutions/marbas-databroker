namespace CraftedSolutions.MarBasSchema
{
    public class SimpleTimeRangeConstraint : ITimeRangeConstraint
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public RangeInclusionFlag Including { get; set; }
    }
}

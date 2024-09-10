namespace MarBasSchema
{
    public interface ITimeRangeConstraint
    {
        DateTime? Start { get; set; }
        DateTime? End { get; set; }
        RangeInclusionFlag Including { get; set; }
    }
}

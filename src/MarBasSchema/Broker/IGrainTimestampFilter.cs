namespace MarBasSchema.Broker
{
    public interface IGrainTimestampFilter
    {
        ITimeRangeConstraint? MTimeConstraint { get; set; }
    }
}

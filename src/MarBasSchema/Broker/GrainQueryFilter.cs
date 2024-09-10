namespace MarBasSchema.Broker
{
    public class GrainQueryFilter: GrainBasicFilter, IGrainQueryFilter
    {
        public ITimeRangeConstraint? MTimeConstraint { get; set; }
    }
}

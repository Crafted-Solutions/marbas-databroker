namespace MarBasSchema
{
    [Flags]
    public enum RangeInclusionFlag
    {
        None = 0x0,
        Start = 0x1,
        End = 0x2,
        Both = Start | End
    }
}

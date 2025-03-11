namespace CraftedSolutions.MarBasSchema.Broker
{
    [Flags]
    public enum GrainCloneDepth
    {
        Self = 0x1, Immediate = 0x2, Infinite = 0x4, Recursive = Self | Infinite
    }
}

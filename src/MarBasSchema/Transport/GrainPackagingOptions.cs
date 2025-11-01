namespace CraftedSolutions.MarBasSchema.Transport
{
    public sealed class GrainPackagingOptions: IGrainPackagingOptions
    {
        public int Priority { get; set; } = 0;
        public ReferenceDepth LinksTraversal { get; set; } = ReferenceDepth.Immediate;
        public ReferenceDepth TypeDefTraversal { get; set; }
        public ReferenceDepth ParentTraversal { get; set; }
        public ReferenceDepth ChildrenTraversal { get; set; }

        public IGrainPackagingOptions Clone()
        {
            return (IGrainPackagingOptions)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}

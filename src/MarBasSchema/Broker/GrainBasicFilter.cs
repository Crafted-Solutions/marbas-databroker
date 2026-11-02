namespace CraftedSolutions.MarBasSchema.Broker
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class GrainBasicFilter : IGrainBasicFilter
    {
        public IEnumerable<ITypeConstraint>? TypeConstraints { get; set; }
        public IEnumerable<Guid>? IdConstraints { get; set; }
    }
}

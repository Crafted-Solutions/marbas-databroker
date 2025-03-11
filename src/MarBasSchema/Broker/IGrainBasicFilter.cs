using CraftedSolutions.MarBasSchema;

namespace CraftedSolutions.MarBasSchema.Broker
{
    public interface IGrainBasicFilter
    {
        IEnumerable<ITypeConstraint>? TypeConstraints { get; set; }
        IEnumerable<Guid>? IdConstraints { get; set; }
    }
}

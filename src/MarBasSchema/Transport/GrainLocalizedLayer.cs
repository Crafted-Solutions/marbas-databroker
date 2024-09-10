
namespace MarBasSchema.Transport
{
    public class GrainLocalizedLayer : IGrainLocalizedLayer
    {
        public string? Label { get; set; }
        public IEnumerable<ITraitTransportable>? Traits { get; set; }
    }
}

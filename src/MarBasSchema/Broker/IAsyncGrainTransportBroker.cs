using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;
using CraftedSolutions.MarBasSchema.Transport;

namespace CraftedSolutions.MarBasSchema.Broker
{
    public interface IAsyncGrainTransportBroker
    {
        Task<IEnumerable<IGrainTransportable>> ExportGrainsAsync(IEnumerable<IIdentifiable> grains, CancellationToken cancellationToken = default);
        Task<IEnumerable<IGrainTransportable>> ExportGrainsAsync(Func<CancellationToken, Task<IEnumerable<IGrain>>> grainEnumerator, CancellationToken cancellationToken = default);
        Task<IGrainImportResults> ImportGrainsAsync(IEnumerable<IGrainTransportable> grains, IEnumerable<IIdentifiable>? grainsToDelete = null, DuplicatesHandlingStrategy duplicatesHandling = DuplicatesHandlingStrategy.Merge, CancellationToken cancellationToken = default);
        Task<int> UpdateGrainTimestampsAsync(IEnumerable<IIdentifiable> grains, DateTime? timestamp = null, CancellationToken cancellationToken = default);
    }
}

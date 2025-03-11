using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Transport;

namespace CraftedSolutions.MarBasSchema.Broker
{
    public interface IGrainTransportBroker
    {
        IEnumerable<IGrainTransportable> ExportGrains(IEnumerable<IIdentifiable> grains);
        IGrainImportResults ImportGrains(IEnumerable<IGrainTransportable> grains, IEnumerable<IIdentifiable>? grainsToDelete = null, DuplicatesHandlingStrategy duplicatesHandling = DuplicatesHandlingStrategy.Merge);
        int UpdateGrainTimestamps(IEnumerable<IIdentifiable> grains, DateTime? timestamp = null);
    }
}

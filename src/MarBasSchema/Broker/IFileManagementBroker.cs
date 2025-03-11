using System.Globalization;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.GrainTier;

namespace CraftedSolutions.MarBasSchema.Broker
{
    public interface IFileManagementBroker
    {
        IGrainFile? GetGrainFile(Guid id, GrainFileContentAccess loadContent = GrainFileContentAccess.OnDemand, CultureInfo? culture = null);
        IGrainFile? CreateGrainFile(string name, string mimeType, Stream content, IIdentifiable? parent = null, long size = -1);
        int StoreGrainFiles(IEnumerable<IGrainFile> files);
    }
}

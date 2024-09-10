using System.Globalization;
using MarBasCommon;
using MarBasSchema.GrainTier;

namespace MarBasSchema.Broker
{
    public interface IFileManagementBroker
    {
        IGrainFile? GetGrainFile(Guid id, GrainFileContentAccess loadContent = GrainFileContentAccess.OnDemand, CultureInfo? culture = null);
        IGrainFile? CreateGrainFile(string name, string mimeType, Stream content, IIdentifiable? parent = null, long size = -1);
        int StoreGrainFiles(IEnumerable<IGrainFile> files);
    }
}

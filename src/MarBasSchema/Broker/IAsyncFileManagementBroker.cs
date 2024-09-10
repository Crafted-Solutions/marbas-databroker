using System.Globalization;
using MarBasCommon;
using MarBasSchema.GrainTier;

namespace MarBasSchema.Broker
{
    public interface IAsyncFileManagementBroker
    {
        Task<IGrainFile?> GetGrainFileAsync(Guid id, GrainFileContentAccess loadContent = GrainFileContentAccess.OnDemand, CultureInfo? culture = null, CancellationToken cancellationToken = default);
        Task<IGrainFile?> CreateGrainFileAsync(string name, string mimeType, Stream content, IIdentifiable? parent = null, long size = -1, CancellationToken cancellationToken = default);
        Task<int> StoreGrainFilesAsync(IEnumerable<IGrainFile> files, CancellationToken cancellationToken = default);
    }
}

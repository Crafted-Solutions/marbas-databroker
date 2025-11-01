using CraftedSolutions.MarBasCommon.Job;

namespace CraftedSolutions.MarBasSchema.Transport
{
    public interface IAsyncSchemaPackager
    {
        Task<Stream> ExportPackageAsync(IDictionary<Guid, IGrainPackagingOptions> packageDefinition, CancellationToken cancellationToken = default);
        Task<IBackgroundJob> SchedulePackageImportAsync(Stream packageStream, DuplicatesHandlingStrategy duplicatesHandling = DuplicatesHandlingStrategy.MergeSkipNewer, MissingDependencyHandlingStrategy missingDependencyHandling = MissingDependencyHandlingStrategy.CreatePlaceholder, CancellationToken cancellationToken = default);
    }
}

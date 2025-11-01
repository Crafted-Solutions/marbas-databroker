using CraftedSolutions.MarBasCommon.Job;

namespace CraftedSolutions.MarBasSchema.Transport
{
    public interface ISchemaPackager
    {
        Stream ExportPackage(IDictionary<Guid, IGrainPackagingOptions> packageDefinition);
        IBackgroundJob SchedulePackageImport(Stream packageStream, DuplicatesHandlingStrategy duplicatesHandling = DuplicatesHandlingStrategy.MergeSkipNewer, MissingDependencyHandlingStrategy missingDependencyHandling = MissingDependencyHandlingStrategy.CreatePlaceholder);
    }
}

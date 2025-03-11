namespace CraftedSolutions.MarBasSchema.Transport
{
    public enum DuplicatesHandlingStrategy
    {
        Ignore,
        MergeSkipNewer,
        Merge,
        OverwriteSkipNewer,
        Overwrite,
        OverwriteRecursive
    }
}

namespace CraftedSolutions.MarBasSchema.IO
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class StorageQuotaExceededException : Exception
    {
        public StorageQuotaExceededException()
        {
        }

        public StorageQuotaExceededException(string? message) : base(message)
        {
        }

        public StorageQuotaExceededException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}

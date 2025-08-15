using System.Runtime.Serialization;

namespace CraftedSolutions.MarBasSchema.Access
{
    public interface ISandboxManager
    {
        int SandboxCount { get; }
        int SandboxMaxCount { get; }
        bool IsAvailable(string sandboxName);
        string AcquireSandbox(string sandboxName);
        Task<string> AcquireSandboxAsync(string sandboxName, CancellationToken cancellationToken = default);
        Task<bool> TrimOldest(CancellationToken cancellationToken = default);
    }

    public class SandboxException : Exception
    {
        public SandboxException()
        {
        }

        public SandboxException(string? message) : base(message)
        {
        }

        public SandboxException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}

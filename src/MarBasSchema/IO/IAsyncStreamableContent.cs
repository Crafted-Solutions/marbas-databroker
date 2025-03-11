namespace CraftedSolutions.MarBasSchema.IO
{
    public interface IAsyncStreamableContent : IStreamableContent
    {
        Task<Stream> GetStreamAsync(CancellationToken cancellationToken = default);
    }
}

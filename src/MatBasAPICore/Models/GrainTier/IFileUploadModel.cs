using Microsoft.AspNetCore.Http;

namespace CraftedSolutions.MarBasAPICore.Models.GrainTier
{
    public interface IFileUploadModel
    {
        IFormFile File { get; }
    }
}

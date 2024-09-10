using Microsoft.AspNetCore.Http;

namespace MarBasAPICore.Models.GrainTier
{
    public interface IFileUploadModel
    {
        IFormFile File { get; }
    }
}

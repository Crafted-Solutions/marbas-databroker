using CraftedSolutions.MarBasSchema.Transport;
using Microsoft.AspNetCore.Http;

namespace CraftedSolutions.MarBasAPICore.Models.Transport
{
    public interface IPackageImportModel
    {
        Guid? NewParentId { get; set; }
        DuplicatesHandlingStrategy? DuplicatesHandling { get; set; }
        MissingDependencyHandlingStrategy? MissingDependencyHandling { get; set; }
        IFormFile Content { get; }
    }
}

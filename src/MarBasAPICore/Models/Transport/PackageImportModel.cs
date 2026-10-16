using CraftedSolutions.MarBasSchema.Transport;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CraftedSolutions.MarBasAPICore.Models.Transport
{
    public class PackageImportModel : IPackageImportModel
    {
        public Guid? NewParentId { get; set; }
        public DuplicatesHandlingStrategy? DuplicatesHandling { get; set; }
        public MissingDependencyHandlingStrategy? MissingDependencyHandling { get; set; }

        [Required]
        public required IFormFile Content { get; set; }
    }
}

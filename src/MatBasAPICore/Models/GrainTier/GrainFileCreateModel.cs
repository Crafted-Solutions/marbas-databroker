using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MarBasAPICore.Models.GrainTier
{
    public class GrainFileCreateModel : IFileUploadModel
    {
        private string? _name;
        IFormFile? _file;

        public Guid? ParentId { get; set; }

        public string Name { get => (string.IsNullOrEmpty(_name) ? _file?.FileName ?? _file?.Name : _name)!; set => _name = value; }

        [Required]
        public IFormFile File { get => _file!; set => _file = value; }
    }
}

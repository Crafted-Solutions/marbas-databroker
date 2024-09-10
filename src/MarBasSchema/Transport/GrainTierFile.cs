using System.Net.Mime;
using System.Text.Json.Serialization;
using MarBasSchema.GrainTier;
using MarBasSchema.IO;

namespace MarBasSchema.Transport
{
    public class GrainTierFile : IGrainTierFile
    {
        [JsonConstructor]
        public GrainTierFile() { }

        public GrainTierFile(IFile other)
        {
            MimeType = other.MimeType;
            Size = other.Size;
            Content = other.Content;
        }

        public string MimeType { get; set; } = MediaTypeNames.Application.Octet;

        public long Size { get; set; } = 0;

        public IStreamableContent? Content { get; set; }
    }
}

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CraftedSolutions.MarBasSchema.IO
{
    public interface IStreamableContent : IDisposable
    {
        byte[]? Data { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        Stream Stream { get; set; }
        long Length { get; }
    }
}

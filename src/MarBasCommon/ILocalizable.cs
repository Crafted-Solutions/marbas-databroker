using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MarBasCommon
{
    public interface ILocalizable
    {
        [ReadOnly(true)]
        [JsonIgnore]
        [IgnoreDataMember]
        CultureInfo? CultureInfo { get; }
        string? Culture { get; }
    }
}

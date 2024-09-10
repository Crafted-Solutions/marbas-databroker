using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MarBasSchema
{
    public interface IUpdateable
    {
        [JsonIgnore]
        [IgnoreDataMember]
        UpdateableTracker FieldTracker { get; }
        ISet<string> GetDirtyFields<TScope>();
    }
}

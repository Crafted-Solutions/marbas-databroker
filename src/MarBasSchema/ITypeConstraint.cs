using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MarBasCommon;

namespace MarBasSchema
{
    public interface ITypeConstraint: ITyped
    {
        string? TypeName { get; }
        [JsonIgnore]
        [IgnoreDataMember]
        IIdentifiable? TypeDef { get; set; }
    }
}

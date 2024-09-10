using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MarBasSchema.Grain;

namespace MarBasSchema.Transport
{
    [JsonDerivedType(typeof(TraitTransportableText), typeDiscriminator: (int)TraitValueType.Text)]
    [JsonDerivedType(typeof(TraitTransportableMemo), typeDiscriminator: (int)TraitValueType.Memo)]
    [JsonDerivedType(typeof(TraitTransportableNumber), typeDiscriminator: (int)TraitValueType.Number)]
    [JsonDerivedType(typeof(TraitTransportableBoolean), typeDiscriminator: (int)TraitValueType.Boolean)]
    [JsonDerivedType(typeof(TraitTransportableDateTime), typeDiscriminator: (int)TraitValueType.DateTime)]
    [JsonDerivedType(typeof(TraitTransportableGrain), typeDiscriminator: (int)TraitValueType.Grain)]
    [JsonDerivedType(typeof(TraitTransportableFile), typeDiscriminator: (int)TraitValueType.File)]
    [JsonDerivedType(typeof(TraitTransportable))]
    public interface ITraitTransportable: ITrait
    {
        [JsonIgnore]
        [IgnoreDataMember]
        new Guid? GrainId { get; }
        [JsonIgnore]
        [IgnoreDataMember]
        new string? Culture { get; set; }
    }
}

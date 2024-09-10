
using System.Text.Json.Serialization;
using MarBasSchema.GrainDef;

namespace MarBasSchema.Transport
{
    public class GrainTierPropDef : IGrainTierPropDef
    {
        [JsonConstructor]
        public GrainTierPropDef() { }

        public GrainTierPropDef(IPropDef other)
        {
            ValueType = other.ValueType;
            CardinalityMin = other.CardinalityMin;
            CardinalityMax = other.CardinalityMax;
            Versionable = other.Versionable;
            Localizable = other.Localizable;
            ValueConstraintId = other.ValueConstraintId;
            ConstraintParams = other.ConstraintParams;
        }

        public Guid? ValueConstraintId { get; set; }

        public string? ConstraintParams { get; set; }
        public int CardinalityMin { get; set; } = 1;
        public int CardinalityMax { get; set; } = 1;
        public bool Versionable { get; set; } = true;
        public bool Localizable { get; set; } = true;

        public TraitValueType ValueType { get; set; } = TraitValueType.Text;
    }
}

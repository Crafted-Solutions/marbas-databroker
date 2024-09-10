using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MarBasCommon;
using MarBasSchema;
using MarBasSchema.Grain;
using MarBasSchema.Grain.Traits;

namespace MarBasAPICore.Models.Trait
{
    public sealed class TraitUpdateModel : ITraitUpdateModel
    {
        private readonly TraitWrapper _trait = new ();

        [Required]
        public Guid Id { get => _trait.Id; set => _trait.Id = value; }

        public Guid? GrainId { get => _trait.GrainId; set => _trait.Grain = ((Identifiable?)value)!; }

        public Guid? PropDefId { get => _trait.PropDefId; set => _trait.PropDef = ((Identifiable?)value)!; }

        public string? Culture { get => _trait.Culture; set => _trait.CultureInfo = (null == value ? null : CultureInfo.GetCultureInfo(value))!; }

        public int? Ord { get => _trait.Ord; set => _trait.Ord = (int)value!; }

        public int? Revision { get => _trait.Revision; set => _trait.Revision = (int)value!; }

        public TraitValueType? ValueType { get => _trait.ValueType; set => _trait.SetValueType(value ?? TraitValueType.Text); }

        public object? Value { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public ITraitBase Trait
        {
            get
            {
                return TraitValueFactory.Create(_trait, Value);
            }
        }

        private class TraitWrapper : TraitBase
        {
            private TraitValueType _valueType;

            public TraitWrapper() : base(new Identifiable(), new Identifiable(), null)
            {
                _valueType = TraitValueType.Text;
                _fieldTracker.AcceptAllChanges = true;
            }

            public new Guid Id { get => base.Id; set => _id = value; }

            public override TraitValueType ValueType { get => _valueType; }

            public void SetValueType(TraitValueType type) => _valueType = type;

            public override object? Value => null;
        }

    }
}

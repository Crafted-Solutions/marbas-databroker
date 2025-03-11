using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Grain;
using CraftedSolutions.MarBasSchema.Grain.Traits;

namespace CraftedSolutions.MarBasAPICore.Models.Trait
{
    public class TraitCreateModel : ITraitCreateModel
    {
        private readonly TraitRef _ref = new();
        private object? _value;

        [Required]
        public Guid GrainId { get => _ref.GrainId; set => _ref.Grain = (Identifiable)value; }

        [Required]
        public Guid PropDefId { get => _ref.PropDefId; set => _ref.PropDef = new SimpleValueTypeContraint((Identifiable)value, ValueType ?? TraitValueType.Text); }

        public string? Culture { get => _ref.Culture; set => _ref.CultureInfo = (null == value ? null : CultureInfo.GetCultureInfo(value))!; }

        public int? Ord { get; set; }

        public int? Revision { get => _ref.Revision; set => _ref.Revision = null == value ? 1 : (int)value; }

        public TraitValueType? ValueType { get => (_ref.PropDef as IValueTypeConstraint)?.ValueType; set => _ref.PropDef = new SimpleValueTypeContraint((Identifiable)PropDefId, value ?? TraitValueType.Text); }

        public object? Value
        {
            get
            {
                if (null == _value)
                {
                    return _value;
                }
                return TraitValueFactory.ConvertValue((TraitValueType)ValueType!, _value);
            }
            set => _value = value;
        }

        [JsonIgnore]
        [IgnoreDataMember]
        public TraitRef Ref => _ref;

        public class TraitRef : ITraitRef
        {
            private IIdentifiable _grain = new Identifiable();
            private IIdentifiable _propdef = new Identifiable();
            private CultureInfo? _culture;
            private int _revision = 1;

            public IIdentifiable Grain { get => _grain; set => _grain = value; }

            public Guid GrainId => _grain.Id;

            public IIdentifiable PropDef { get => _propdef; set => _propdef = value; }

            public Guid PropDefId => _propdef.Id;

            public int Revision { get => _revision; set => _revision = value; }

            public CultureInfo CultureInfo { get => _culture!; set => _culture = value; }

            public string Culture => _culture?.Name!;
        }
    }
}

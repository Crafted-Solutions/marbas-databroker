using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;
using CraftedSolutions.MarBasSchema.Grain.Traits;

namespace CraftedSolutions.MarBasSchema.Transport
{
    public class TraitTransportable : Identifiable, ITraitTransportable
    {
        private Guid _grainId;

        [JsonConstructor]
        public TraitTransportable()
        {
        }

        public TraitTransportable(Guid id, Guid grainId, Guid propDefId)
            : base(id)
        {
            _grainId = grainId;
            PropDefId = propDefId;
        }

        public TraitTransportable(ITrait other)
            : base(other.Id)
        {
            _grainId = other.GrainId;
            PropDefId = other.PropDefId;
            Culture = other.Culture;
            ValueType = other.ValueType;
            Revision = other.Revision;
            Ord = other.Ord;
            Value = other.Value;
        }

        public new Guid Id { get => _id; set => _id = value; }

        [JsonIgnore]
        [IgnoreDataMember]
        public Guid? GrainId => _grainId;
        [JsonIgnore]
        [IgnoreDataMember]
        public IIdentifiable Grain { get => (Identifiable)_grainId; set => _grainId = value.Id; }
        [JsonIgnore]
        [IgnoreDataMember]
        Guid IGrainBinding.GrainId => _grainId;

        public int Ord { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public bool IsNull => null == Value;

        public virtual object? Value { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual TraitValueType ValueType { get; set; } = TraitValueType.Text;

        [JsonIgnore]
        [IgnoreDataMember]
        public IIdentifiable PropDef { get => (Identifiable)PropDefId; set => PropDefId = value.Id; }

        public Guid PropDefId { get; set; }

        public int Revision { get; set; } = 1;



        [JsonIgnore]
        [IgnoreDataMember]
        public string? Culture { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public CultureInfo? CultureInfo => null == Culture ? null : CultureInfo.GetCultureInfo(Culture);

    }

    public class TraitTransportableValue<T> : TraitTransportable, ITraitValue<T>
    {
        protected T? _value;

        [JsonConstructor]
        public TraitTransportableValue()
        {
        }

        public TraitTransportableValue(ITrait other) : base(other)
        {
        }

        public override object? Value { get => _value; set => _value = (T?)TraitValueFactory.ConvertValue(ValueType, value); }
        T? ITraitValue<T>.Value { get => _value; set => _value = value; }

    }

    public class TraitTransportableGuid : TraitTransportableValue<Guid?>
    {
        [JsonConstructor]
        public TraitTransportableGuid()
        {
        }

        public TraitTransportableGuid(ITrait other) : base(other)
        {
        }

        public override object? Value { get => _value; set => _value = ((Identifiable?)TraitValueFactory.ConvertValue(ValueType, value))?.Id; }
    }

    public class TraitTransportableText : TraitTransportableValue<string>
    {
        [JsonConstructor]
        public TraitTransportableText()
        {
        }

        public TraitTransportableText(ITrait other) : base(other)
        {
        }

        [JsonIgnore]
        [IgnoreDataMember]
        public override TraitValueType ValueType => TraitValueType.Text;
    }
    public class TraitTransportableMemo : TraitTransportableValue<string>
    {
        [JsonConstructor]
        public TraitTransportableMemo()
        {
        }

        public TraitTransportableMemo(ITrait other) : base(other)
        {
        }

        [JsonIgnore]
        [IgnoreDataMember]
        public override TraitValueType ValueType => TraitValueType.Memo;
    }
    public class TraitTransportableNumber : TraitTransportableValue<decimal>
    {
        [JsonConstructor]
        public TraitTransportableNumber()
        {
        }

        public TraitTransportableNumber(ITrait other) : base(other)
        {
        }

        [JsonIgnore]
        [IgnoreDataMember]
        public override TraitValueType ValueType => TraitValueType.Number;
    }
    public class TraitTransportableBoolean : TraitTransportableValue<bool>
    {
        [JsonConstructor]
        public TraitTransportableBoolean()
        {
        }

        public TraitTransportableBoolean(ITrait other) : base(other)
        {
        }

        [JsonIgnore]
        [IgnoreDataMember]
        public override TraitValueType ValueType => TraitValueType.Boolean;
    }
    public class TraitTransportableDateTime : TraitTransportableValue<DateTime>
    {
        [JsonConstructor]
        public TraitTransportableDateTime()
        {
        }

        public TraitTransportableDateTime(ITrait other) : base(other)
        {
        }

        [JsonIgnore]
        [IgnoreDataMember]
        public override TraitValueType ValueType => TraitValueType.DateTime;
    }
    public class TraitTransportableGrain : TraitTransportableGuid
    {
        [JsonConstructor]
        public TraitTransportableGrain()
        {
        }

        public TraitTransportableGrain(ITrait other) : base(other)
        {
        }

        [JsonIgnore]
        [IgnoreDataMember]
        public override TraitValueType ValueType => TraitValueType.Grain;
    }
    public class TraitTransportableFile : TraitTransportableGuid
    {
        [JsonConstructor]
        public TraitTransportableFile()
        {
        }

        public TraitTransportableFile(ITrait other) : base(other)
        {
        }

        [JsonIgnore]
        [IgnoreDataMember]
        public override TraitValueType ValueType => TraitValueType.File;
    }
}

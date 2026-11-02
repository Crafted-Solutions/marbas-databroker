using CraftedSolutions.MarBasCommon;
using Microsoft.VisualBasic;
using System.Globalization;

namespace CraftedSolutions.MarBasSchema.Grain.Traits
{
    public class TraitValue<T> : TraitBase, ITraitValue<T>
    {
        protected object? _value;

        public TraitValue(IIdentifiable grain, IIdentifiable propDef, object? value, CultureInfo? culture = null)
            : base(grain, propDef, culture)
        {
            if (value is not null && value is not T)
            {
                throw new ArgumentException($"{nameof(value)} must be {typeof(T).Name}? but acutally is {value.GetType().Name}");
            }
            ThrowIfWrongType();
            _value = value;
        }

        public TraitValue(IIdentifiable grain, IIdentifiable propDef, T? value, CultureInfo? culture = null)
            : base(grain, propDef, culture)
        {
            ThrowIfWrongType();
            _value = value;
        }

        public TraitValue(ITrait other)
            : base(other)
        {
            ThrowIfWrongType();
            if (other is ITraitValue<T> traitValue)
            {
                _value = traitValue.Value;
            }
            else
            {
                _value = other.Value;
            }
        }

        public override object? Value => _value;

        T? ITraitValue<T>.Value
        {
            get => (T?)_value;
            set
            {
                if (_fieldTracker.IsChangeAccepted(_value, value))
                {
                    _value = value;
                    _fieldTracker.TrackPropertyChange<ITraitBase>();
                }
            }
        }

        public override IIdentifiable PropDef
        {
            get => base.PropDef;
            set
            {
                base.PropDef = value;
                ThrowIfWrongType();
            }
        }

        public ITraitValue<T> AsWritable() => (ITraitValue<T>)this;

        protected void ThrowIfWrongType()
        {
            var type = typeof(T);
            var ntype = Nullable.GetUnderlyingType(type);
            var expectedType = TraitValueFactory.GetValueNativeType(base.ValueType);
            if (type != expectedType && ntype != expectedType)
            {
                throw new InvalidCastException($"{nameof(TraitValue<T>)}<{(ntype ?? type).Name}> is incompatible with {nameof(ValueType)} {Enum.GetName(base.ValueType)}");
            }
        }
    }
}

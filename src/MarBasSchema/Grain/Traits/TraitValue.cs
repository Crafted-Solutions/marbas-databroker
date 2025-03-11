using System.Globalization;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema;

namespace CraftedSolutions.MarBasSchema.Grain.Traits
{
    public class TraitValue<T> : TraitBase, ITraitValue<T>
    {
        protected T? _value;

        public TraitValue(IIdentifiable grain, IIdentifiable propdef, T? value, CultureInfo? culture = null)
            : base(grain, propdef, culture)
        {
            _value = value;
            if (PropDef is not IValueTypeConstraint)
            {
                _traitRef.PropDef = new SimpleValueTypeContraint(PropDef);
            }
        }

        public TraitValue(ITraitBase other)
            : base(other)
        {
            if (other is ITraitValue<T> traitValue)
            {
                _value = traitValue.Value;
            }
            else
            {
                _value = (T?)other.Value;
            }
            if (PropDef is not IValueTypeConstraint)
            {
                _traitRef.PropDef = new SimpleValueTypeContraint(PropDef, other.ValueType);
            }
        }

        public override object? Value { get => _value; }

        T? ITraitValue<T>.Value
        {
            get => _value;
            set
            {
                if (!(null == value && null == _value) || null == _value || !_value.Equals(value))
                {
                    _value = value;
                    _fieldTracker.TrackPropertyChange<ITraitValue<T>>();
                }
            }
        }
    }
}

using CraftedSolutions.MarBasCommon;
using System.Globalization;

namespace CraftedSolutions.MarBasSchema.Grain.Traits
{
    public class TraitFile : TraitValue<Guid?>
    {
        public TraitFile(ITrait other)
            : base(other)
        {
            _value = other is ITraitValue<Guid?> val ? val.Value : (Guid?)other.Value;
        }

        public TraitFile(IIdentifiable grain, IIdentifiable propDef, IIdentifiable? value = null, CultureInfo? culture = null)
            : base(grain, propDef is not IValueTypeConstraint ? new SimpleValueTypeContraint(propDef, TraitValueType.File) : propDef, value?.Id, culture)
        {
        }

        public override TraitValueType ValueType => TraitValueType.File;
    }
}

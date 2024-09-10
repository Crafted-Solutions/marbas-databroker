using System.Globalization;
using MarBasCommon;

namespace MarBasSchema.Grain.Traits
{
    public class TraitFile : TraitValue<Guid?>
    {
        public TraitFile(ITraitBase other)
            : base(other)
        {
            _value = (other is ITraitValue<Guid?> val ? val.Value : (Guid?)other.Value);
        }

        public TraitFile(IIdentifiable grain, IIdentifiable propdef, IIdentifiable? value = null, CultureInfo? culture = null)
            : base(grain, propdef, value?.Id, culture)
        {
        }

        public override TraitValueType ValueType => TraitValueType.File;
    }
}

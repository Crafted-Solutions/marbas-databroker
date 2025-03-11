using System.Globalization;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasSchema.Grain.Traits
{
    public class TraitText : TraitValue<string>
    {
        public TraitText(ITraitBase other)
            : base(other)
        {
        }

        public TraitText(IIdentifiable grain, IIdentifiable propdef, string? value = null, CultureInfo? culture = null)
            : base(grain, propdef, value, culture)
        {
        }
    }
}

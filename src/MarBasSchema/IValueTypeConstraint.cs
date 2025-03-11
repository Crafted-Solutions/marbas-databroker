using System.ComponentModel;

namespace CraftedSolutions.MarBasSchema
{
    public enum TraitValueType
    {
        Text, Memo, Number, Boolean, DateTime, File, Grain
    }

    public interface IValueTypeConstraint
    {
        [ReadOnly(true)]
        TraitValueType ValueType { get; }
    }
}

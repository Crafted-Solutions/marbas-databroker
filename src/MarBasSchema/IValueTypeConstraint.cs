using System.ComponentModel;

namespace MarBasSchema
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

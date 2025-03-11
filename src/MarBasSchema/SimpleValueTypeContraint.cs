using CraftedSolutions.MarBasCommon;

namespace CraftedSolutions.MarBasSchema
{
    public class SimpleValueTypeContraint : Identifiable, IValueTypeConstraint
    {
        protected TraitValueType _valueType;

        public SimpleValueTypeContraint(IIdentifiable other, TraitValueType valueType = TraitValueType.Text)
            : base(other)
        {
            ValueType = valueType;
        }

        public TraitValueType ValueType { get => _valueType; set => _valueType = value; }
    }
}

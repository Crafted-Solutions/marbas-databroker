namespace CraftedSolutions.MarBasSchema.Broker
{
    [Flags]
    public enum FieldOperatorNegation
    {
        Not = 0x1000
    }
    public enum FieldCompareOperator
    {
        Eq = 0x0001,
        Gt = 0x0002,
        Gte = Gt | Eq,
        Lt = 0x0004,
        Lte = Lt | Eq,
        Contains = 0x0010,
        StartsWith = 0x0020,
        EndsWith = 0x00040,
        NotEq = Eq | FieldOperatorNegation.Not,
        ContainsNot = Contains | FieldOperatorNegation.Not,
        StartsNotWith = StartsWith | FieldOperatorNegation.Not,
        EndsNotWith = EndsWith | FieldOperatorNegation.Not
    }
    public static class FieldCompareOperatorExtension
    {
        public static bool IsNegated(this FieldCompareOperator compareOperator)
        {
            return 0 < ((int)FieldOperatorNegation.Not & (int)compareOperator);
        }
    }
}

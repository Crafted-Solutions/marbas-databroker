using AwesomeAssertions;
using CraftedSolutions.MarBasSchema.Broker;

namespace CraftedSolutions.MarBasSchema.Tests.Broker
{
    [TestClass]
    public class FieldCompareOperatorTest
    {
        [TestMethod]
        [DataRow(FieldCompareOperator.NotEq)]
        [DataRow(FieldCompareOperator.ContainsNot)]
        [DataRow(FieldCompareOperator.EndsNotWith)]
        [DataRow(FieldCompareOperator.StartsNotWith)]
        public void IsNegated_returning_True_given_value_combibined_with_Not(FieldCompareOperator compareOperator)
        {
            compareOperator.IsNegated().Should().BeTrue();
        }

        [TestMethod]
        [DataRow(FieldCompareOperator.Eq)]
        [DataRow(FieldCompareOperator.Gt)]
        [DataRow(FieldCompareOperator.Gte)]
        [DataRow(FieldCompareOperator.Lt)]
        [DataRow(FieldCompareOperator.Lte)]
        [DataRow(FieldCompareOperator.Contains)]
        [DataRow(FieldCompareOperator.StartsWith)]
        [DataRow(FieldCompareOperator.EndsWith)]
        public void IsNegated_returning_False_given_value_not_combibined_with_Not(FieldCompareOperator compareOperator)
        {
            compareOperator.IsNegated().Should().BeFalse();
        }
    }
}

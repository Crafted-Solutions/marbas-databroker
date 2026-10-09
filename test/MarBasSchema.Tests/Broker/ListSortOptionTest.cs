using AwesomeAssertions;
using CraftedSolutions.MarBasSchema.Broker;

namespace CraftedSolutions.MarBasSchema.Tests.Broker
{
    [TestClass]
    public class ListSortOptionTest
    {
        [TestMethod]
        [DataRow(GrainSortField.Id)]
        [DataRow(RoleSortField.Id)]
        public void CTor_setting_defaults_given_no_parameters<T>(T _)
            where T : struct, Enum
        {
            var option = new ListSortOption<T>();

            option.Field.Should().Be(Enum.GetValues<T>()[0]);
            option.Order.Should().Be(ListSortOrder.Asc);
        }

        [TestMethod]
        [DataRow(GrainSortField.Id, ListSortOrder.Asc, DisplayName = "CTor_setting_GrainSortField_and_Asc_Order")]
        [DataRow(GrainSortField.Id, ListSortOrder.Desc, DisplayName = "CTor_setting_GrainSortField_and_Desc_Order")]
        [DataRow(RoleSortField.Id, ListSortOrder.Asc, DisplayName = "CTor_setting_RoleSortField_and_Asc_Order")]
        [DataRow(RoleSortField.Id, ListSortOrder.Desc, DisplayName = "CTor_setting_RoleSortField_and_Desc_Order")]
        public void CTor_setting_Field_and_Order<T>(T _, ListSortOrder sortOrder)
            where T : struct, Enum
        {
            foreach(var field in Enum.GetValues<T>())
            {
                var option = new ListSortOption<T>(field, sortOrder);

                option.Field.Should().Be(field);
                option.Order.Should().Be(sortOrder);
            }
        }
    }
}

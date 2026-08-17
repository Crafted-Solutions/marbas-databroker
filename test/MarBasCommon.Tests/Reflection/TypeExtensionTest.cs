using AwesomeAssertions;
using CraftedSolutions.MarBasCommon.Job;
using CraftedSolutions.MarBasCommon.Reflection;
using System.Reflection;

namespace CraftedSolutions.MarBasCommon.Tests.Reflection
{
    [TestClass]
    public class TypeExtensionTest
    {
        [TestMethod]
        public void GetEnumerableType_returning_concrete_Type_of_element_given_IEnumerable_and_valid_index()
        {
            var list = new List<ILabeled>();
            list.GetType().GetEnumerableType().Should().Be<ILabeled>();
            
            var set = new HashSet<IConfigurable>();
            set.GetType().GetEnumerableType().Should().Be<IConfigurable>();
            
            var map = new Dictionary<string, ILocalizable>();
            map.GetType().GetEnumerableType(1).Should().Be<ILocalizable>();
        }
        [TestMethod]
        public void GetEnumerableType_returning_Type_of_object_given_invalid_index()
        {
            var map = new List<ILocalizable>();
            map.GetType().GetEnumerableType(1).Should().Be<object>();
        }
        [TestMethod]
        public void GetEnumerableType_returning_Type_of_object_given_anything_but_IEnumerable()
        {
            var obj = new NamedIdentifiable(Guid.NewGuid(), "test");
            obj.GetType().GetEnumerableType().Should().Be<object>();
        }

        [TestMethod]
        public void GetAllProperties_returning_empty_collection_given_null_Type()
        {
            Type? nullable = null;
            nullable.GetAllProperties().Should().BeEmpty();
        }
        [TestMethod]
        public void GetAllProperties_returning_public_properties_of_Interface()
        {
            var properties = typeof(IBackgroundJobState).GetAllProperties();
            properties.Should().NotBeEmpty().And.Satisfy(
                property => nameof(IBackgroundJobState.Stage) == property.Name && typeof(string) == property.PropertyType,
                property => nameof(IBackgroundJobState.Progress) == property.Name && typeof(int) == property.PropertyType,
                property => nameof(IBackgroundJobState.Status) == property.Name && typeof(BackgroundJobStatus) == property.PropertyType,
                property => nameof(IBackgroundJobState.Result) == property.Name && typeof(object) == property.PropertyType
                );
        }
        [TestMethod]
        public void GetAllProperties_returning_public_properties_of_Class()
        {
            var properties = typeof(BackgroundJob).GetAllProperties();
            properties.Should().NotBeEmpty().And.Satisfy(
                property => nameof(BackgroundJob.Id) == property.Name && typeof(Guid) == property.PropertyType,
                property => nameof(BackgroundJob.Name) == property.Name && typeof(string) == property.PropertyType,
                property => nameof(BackgroundJob.Owner) == property.Name && typeof(string) == property.PropertyType,
                property => nameof(BackgroundJob.Flags) == property.Name && typeof(BackgroundJobFlags) == property.PropertyType,
                property => nameof(BackgroundJob.Created) == property.Name && typeof(DateTime) == property.PropertyType,
                property => nameof(BackgroundJob.Stage) == property.Name && typeof(string) == property.PropertyType,
                property => nameof(BackgroundJob.Progress) == property.Name && typeof(int) == property.PropertyType,
                property => nameof(BackgroundJob.Status) == property.Name && typeof(BackgroundJobStatus) == property.PropertyType,
                property => nameof(BackgroundJob.Result) == property.Name && typeof(object) == property.PropertyType,
                property => nameof(BackgroundJob.Started) == property.Name && typeof(DateTime?) == property.PropertyType
                );
        }
        [TestMethod]
        public void GetAllProperties_returning_public_properties_of_Interface_recursively()
        {
            var properties = typeof(IBackgroundJob).GetAllProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            properties.Should().NotBeEmpty().And.Satisfy(
                property => nameof(IBackgroundJob.Id) == property.Name && typeof(Guid) == property.PropertyType,
                property => nameof(IBackgroundJob.Name) == property.Name && typeof(string) == property.PropertyType,
                property => nameof(IBackgroundJob.Owner) == property.Name && typeof(string) == property.PropertyType,
                property => nameof(IBackgroundJob.Flags) == property.Name && typeof(BackgroundJobFlags) == property.PropertyType,
                property => nameof(IBackgroundJob.Created) == property.Name && typeof(DateTime) == property.PropertyType,
                property => nameof(IBackgroundJob.Stage) == property.Name && typeof(string) == property.PropertyType,
                property => nameof(IBackgroundJob.Progress) == property.Name && typeof(int) == property.PropertyType,
                property => nameof(IBackgroundJob.Status) == property.Name && typeof(BackgroundJobStatus) == property.PropertyType,
                property => nameof(IBackgroundJob.Result) == property.Name && typeof(object) == property.PropertyType,
                property => nameof(IBackgroundJob.Started) == property.Name && typeof(DateTime?) == property.PropertyType
                );
        }
    }
}

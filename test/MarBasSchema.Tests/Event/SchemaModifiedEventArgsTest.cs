using AwesomeAssertions;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Event;
using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasSchema.Tests.Event
{
    [TestClass]
    public class SchemaModifiedEventArgsTest
    {
        [TestMethod]
        public void CTor_setting_ChangeType()
        {
            var args = new SchemaModifiedEventArgs<IIdentifiable>(SchemaModificationType.Update);

            args.ChangeType.Should().Be(SchemaModificationType.Update);
            args.Subjects.Should().BeEmpty();
            args.ConcreteSubjectType.Should().Be<IIdentifiable>();
        }

        [TestMethod]
        [DataRow(typeof(GrainPlain), SchemaModificationType.Create)]
        [DataRow(typeof(NamedIdentifiable), SchemaModificationType.Delete)]
        public void CTor_setting_ChangeType_and_Subjects(Type subjectType, SchemaModificationType modificationType)
        {
            var subjects = new object[] { Activator.CreateInstance(subjectType)!, Activator.CreateInstance(subjectType)! };
            var args = new SchemaModifiedEventArgs<IIdentifiable>(modificationType, subjects.Cast<IIdentifiable>());

            args.ChangeType.Should().Be(modificationType);
            args.Subjects.Should().AllBeAssignableTo(subjectType).And.HaveCount(2);
            args.ConcreteSubjectType.Should().Be<IIdentifiable>();
        }

        [TestMethod]
        [DataRow(typeof(GrainPlain), SchemaModificationType.Create)]
        [DataRow(typeof(NamedIdentifiable), SchemaModificationType.Delete)]
        public void CTor_setting_ChangeType_Subjects_and_ConcreteSubjectType(Type subjectType, SchemaModificationType modificationType)
        {
            var subjects = new object[] { Activator.CreateInstance(subjectType)!, Activator.CreateInstance(subjectType)! };
            var args = new SchemaModifiedEventArgs<IIdentifiable>(modificationType, subjects.Cast<IIdentifiable>(), subjectType);

            args.ChangeType.Should().Be(modificationType);
            args.Subjects.Should().AllBeAssignableTo(subjectType).And.HaveCount(2);
            args.ConcreteSubjectType.Should().Be(subjectType);
        }

        [TestMethod]
        public void AddSubject_adding_distinct_Subjects()
        {
            var args = new SchemaModifiedEventArgs<IIdentifiable>(SchemaModificationType.Create);
            var role = new SchemaRole();

            args.AddSubject(role);
            args.AddSubject(new SchemaRole());
            args.Subjects.Should().HaveCount(2);

            args.AddSubject(role);
            args.Subjects.Should().HaveCount(2, "filtered objects with the same ID");
        }

        [TestMethod]
        public void AddSubject_throwing_ArgumentException_given_incompatible_ConcreteSubjectType()
        {
            var args = new SchemaModifiedEventArgs<IIdentifiable>(SchemaModificationType.Create, concreteSubjectType: typeof(IGrain));

            args.Invoking(args => args.AddSubject(new SchemaRole())).Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void RemoveSubject_removing_elements_from_Subjects()
        {
            var roles = new[] { new SchemaRole(), new SchemaRole() };
            var args = new SchemaModifiedEventArgs<ISchemaRole>(SchemaModificationType.Update, roles);

            args.Subjects.Should().HaveCount(2);

            args.RemoveSubject(roles[0]);
            args.Subjects.Should().HaveCount(1);

            args.RemoveSubject(roles[0]);
            args.Subjects.Should().HaveCount(1, "element already removed");

            args.RemoveSubject(new SchemaRole(roles[1]));
            args.Subjects.Should().BeEmpty();
        }
    }
}

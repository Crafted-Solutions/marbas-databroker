using AwesomeAssertions;
using CraftedSolutions.MarBasCommon;

namespace CraftedSolutions.MarBasSchema.Tests
{
    [TestClass]
    public class SimpleTypeConstraintTest
    {
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        class TypeConstraintMock(Guid typeDefId) : ITypeConstraint
        {
            IIdentifiable? _typeDef = (Identifiable)typeDefId;

            public string? TypeName => null;

            public IIdentifiable? TypeDef { get => _typeDef; set => _typeDef = value; }

            public Guid? TypeDefId => _typeDef?.Id;
        }

        [TestMethod]
        public void CTor_storing_TypeDefId()
        {
            var typeDefId = Guid.NewGuid();
            var constraint = new SimpleTypeConstraint(typeDefId);

            constraint.TypeDefId.Should().Be(typeDefId);
            constraint.TypeDef.Should().NotBeNull();
            constraint.TypeDef.Id.Should().Be(typeDefId);
        }

        [TestMethod]
        public void CTor_storing_TypeDefId_and_Name()
        {
            var typeDefId = Guid.NewGuid();
            var name = "TestType";
            var constraint = new SimpleTypeConstraint(typeDefId, name);

            constraint.TypeDefId.Should().Be(typeDefId);
            constraint.TypeName.Should().Be(name);
            constraint.TypeDef.Should().NotBeNull();
            constraint.TypeDef.Id.Should().Be(typeDefId);
            constraint.TypeDef.Should().BeAssignableTo<INamed>();
            ((INamed)constraint.TypeDef).Name.Should().Be(name);
        }

        [TestMethod]
        public void CTor_copying_ITypeConstraint()
        {
            var origin = new TypeConstraintMock(Guid.NewGuid());
            var constraint = new SimpleTypeConstraint(origin);

            constraint.TypeDefId.Should().Be(origin.TypeDefId);
            constraint.TypeDef.Should().NotBeNull();
            constraint.TypeDef.Id.Should().Be(origin.TypeDef!.Id);
            constraint.TypeDef.Should().NotBeAssignableTo<INamed>();
            constraint.TypeName.Should().BeNull();
        }

        [TestMethod]
        public void CTor_copying_ITypeConstraint_with_TypeName()
        {
            var origin = new SimpleTypeConstraint(Guid.NewGuid(), "TestType");
            var constraint = new SimpleTypeConstraint(origin);

            constraint.TypeDefId.Should().Be(origin.TypeDefId);
            constraint.TypeName.Should().Be(origin.TypeName);
            constraint.TypeDef.Should().NotBeNull();
            constraint.TypeDef.Id.Should().Be(origin.TypeDef!.Id);
            constraint.TypeDef.Should().BeAssignableTo<INamed>();
            ((INamed)constraint.TypeDef).Name.Should().Be(origin.TypeName);
        }

        [TestMethod]
        public void Set_TypeDef_changing_TypeDefId_and_TypeName()
        {
            var constraint = new SimpleTypeConstraint();

            constraint.TypeDefId.Should().BeNull();
            constraint.TypeName.Should().BeNull();
            constraint.TypeDef.Should().BeNull();

            var typeDef = new NamedIdentifiable(Guid.NewGuid(), "TestType");
            constraint.TypeDef = typeDef;

            constraint.TypeDefId.Should().Be(typeDef.Id);
            constraint.TypeName.Should().Be(typeDef.Name);
            constraint.TypeDef.Should().NotBeNull();
            constraint.TypeDef.Id.Should().Be(typeDef.Id);
            constraint.TypeDef.Should().BeAssignableTo<INamed>();
            ((INamed)constraint.TypeDef).Name.Should().Be(typeDef.Name);
        }

        [TestMethod]
        public void Set_TypeDefId_replacing_TypeDef()
        {
            var typeDefId = Guid.NewGuid();
            var name = "TestType";

            var constraint = new SimpleTypeConstraint(typeDefId, name);
            constraint.TypeDefId.Should().Be(typeDefId);
            constraint.TypeName.Should().Be(name);

            constraint.TypeDefId = Guid.Empty;

            constraint.TypeDef.Should().NotBeNull();
            constraint.TypeName.Should().BeNull();
            constraint.TypeDefId.Should().BeNull();

            constraint.TypeDefId = null;

            constraint.TypeDef.Should().BeNull();
            constraint.TypeName.Should().BeNull();
            constraint.TypeDefId.Should().BeNull();
        }

        [TestMethod]
        public void CreateFrom_creating_instance_from_typeDef()
        {
            var typeDef = new NamedIdentifiable(Guid.NewGuid(), "TestType");
            var constraint = SimpleTypeConstraint.CreateFrom(typeDef);

            constraint.TypeDefId.Should().Be(typeDef.Id);
            constraint.TypeName.Should().Be(typeDef.Name);
            constraint.TypeDef.Should().NotBeNull();
            constraint.TypeDef.Id.Should().Be(typeDef.Id);
            constraint.TypeDef.Should().BeAssignableTo<INamed>();
            ((INamed)constraint.TypeDef).Name.Should().Be(typeDef.Name);
        }

        [TestMethod]
        public void GetHashCode_returning_same_value_given_equivalent_ITypeConstraint()
        {
            var typeDefId = Guid.NewGuid();
            var typeName = "TestType";
            var constraint = new SimpleTypeConstraint(typeDefId, typeName);
            constraint.GetHashCode().Should().Be(new SimpleTypeConstraint(typeDefId, typeName).GetHashCode());
        }

        [TestMethod]
        public void GetHashCode_returning_different_value_given_different_TypeDefId()
        {
            var typeName = "TestType";
            var constraint = new SimpleTypeConstraint(Guid.NewGuid(), typeName);
            constraint.GetHashCode().Should().NotBe(new SimpleTypeConstraint(Guid.NewGuid(), typeName).GetHashCode());
        }

        [TestMethod]
        public void GetHashCode_returning_different_value_given_different_TypeName()
        {
            var typeDefId = Guid.NewGuid();
            var constraint = new SimpleTypeConstraint(typeDefId, "TestType1");
            constraint.GetHashCode().Should().NotBe(new SimpleTypeConstraint(typeDefId, "TestType2").GetHashCode());
        }

        [TestMethod]
        public void Equals_returning_false_given_comparand_of_other_Type()
        {
            var constraint = new SimpleTypeConstraint();
            constraint.Equals("anything").Should().BeFalse();
            constraint.Equals(null).Should().BeFalse();
            (constraint == (ITypeConstraint?)null).Should().BeFalse();
            ((SimpleTypeConstraint?)null == constraint).Should().BeFalse();
        }

        [TestMethod]
        public void Equals_returning_false_given_ITypeConstraint_having_wrong_TypeDefId()
        {
            var typeName = "TestType";
            var constraint = new SimpleTypeConstraint(Guid.NewGuid(), typeName);
            var other = new SimpleTypeConstraint(Guid.NewGuid(), typeName);
            constraint.Equals(other).Should().BeFalse();
            (constraint == other).Should().BeFalse();
            (constraint != other).Should().BeTrue();
        }

        [TestMethod]
        public void Equals_returning_false_given_ITypeConstraint_having_wrong_TypeName()
        {
            var typeDefId = Guid.NewGuid();
            var constraint = new SimpleTypeConstraint(typeDefId, "TestType1");
            var other = new SimpleTypeConstraint(typeDefId, "TestType2");
            constraint.Equals(other).Should().BeFalse();
            (constraint == other).Should().BeFalse();
            (constraint != other).Should().BeTrue();
        }

        [TestMethod]
        public void Equals_returning_false_given_unrelated_ITypeConstraints()
        {
            var constraint = new SimpleTypeConstraint(Guid.NewGuid(), "TestType1");
            var other = new SimpleTypeConstraint(Guid.NewGuid());
            constraint.Equals(other).Should().BeFalse();
            (constraint == other).Should().BeFalse();
            (constraint != other).Should().BeTrue();
        }

        [TestMethod]
        public void Equals_returning_true_given_equivalent_ITypeConstraint()
        {
            var typeDefId = Guid.NewGuid();
            var typeName = "TestType";
            var constraint = new SimpleTypeConstraint(typeDefId, typeName);
            var other = new SimpleTypeConstraint(typeDefId, typeName);
            constraint.Equals(constraint).Should().BeTrue();
#pragma warning disable CS1718 // Comparison made to same variable
            (constraint == constraint).Should().BeTrue();
            (constraint != constraint).Should().BeFalse();
#pragma warning restore CS1718 // Comparison made to same variable
            constraint.Equals(other).Should().BeTrue();
            (constraint == other).Should().BeTrue();
            (constraint != other).Should().BeFalse();
        }
    }
}

using AwesomeAssertions;
using CraftedSolutions.MarBasCommon.Reflection;
using System.Reflection;
using System.Text.Json;

namespace CraftedSolutions.MarBasCommon.Tests.Reflection
{
    [TestClass]
    public sealed class ObjectExtensionTest
    {
        [TestMethod]
        public void CastTo_returning_object_of_declared_Type()
        {
            object obj = new Identifiable();
            var casted = obj.CastTo<Identifiable>();
            casted.Should().NotBeNull().And.BeOfType<Identifiable>();
        }
        [TestMethod]
        public void CastTo_returning_object_of_interface_Type()
        {
            object obj = new NamedIdentifiable();
            var firstInterface = obj.CastTo<IIdentifiable>();
            firstInterface.Should().NotBeNull().And.BeAssignableTo<IIdentifiable>();
            var secondInterface = obj.CastTo<INamed>();
            secondInterface.Should().NotBeNull().And.BeAssignableTo<INamed>();
        }
        [TestMethod]
        public void CastTo_throwing_Exception_given_incompatible_Type()
        {
            object obj = new Identifiable();
            obj.Invoking(x => x.CastTo<ILabeled>()).Should().Throw<InvalidCastException>();
        }

        [TestMethod]
        public void CastToReflected_returning_object_of_declared_Type()
        {
            object obj = new Identifiable();
            var casted = obj.CastToReflected(typeof(Identifiable));
            ((object?)casted).Should().NotBeNull().And.BeOfType<Identifiable>();
        }
        [TestMethod]
        public void CastToReflected_returning_object_of_interface_Type()
        {
            object obj = new NamedIdentifiable();
            var firstInterface = obj.CastToReflected(typeof(IIdentifiable));
            ((object?) firstInterface).Should().NotBeNull().And.BeAssignableTo<IIdentifiable>();
            var secondInterface = obj.CastToReflected(typeof(INamed));
            ((object?)secondInterface).Should().NotBeNull().And.BeAssignableTo<INamed>();
        }
        [TestMethod]
        public void CastToReflected_throwing_Exception_given_incompatible_Type()
        {
            object obj = new Identifiable();
            obj.Invoking(x => x.CastToReflected(typeof(ILabeled))).Should().Throw<TargetInvocationException>();
        }

        [TestMethod]
        public void CastUnparsedJson_returning_object_itsself_given_object_is_anything_but_JsonElement()
        {
            var intObj = 1;
            intObj.CastUnparsedJson().Should().Be(intObj);
            var boolObj = true;
            boolObj.CastUnparsedJson().Should().Be(boolObj);
            var obj = new Identifiable();
            obj.CastUnparsedJson().Should().BeSameAs(obj);
            object? nullable = null;
            nullable.CastUnparsedJson().Should().BeNull();
        }
        [TestMethod]
        public void CastUnparsedJson_returning_decimal_given_object_is_of_JsonValueKind_Number()
        {
            var castedDecimal = JsonElement.Parse("3.14").CastUnparsedJson();
            castedDecimal.Should().Be(3.14M);
        }
        [TestMethod]
        public void CastUnparsedJson_returning_bool_given_object_is_of_JsonValueKind_True_or_False()
        {
            var castedBool = JsonElement.Parse("true").CastUnparsedJson();
            castedBool.Should().Be(true);
            castedBool = JsonElement.Parse("false").CastUnparsedJson();
            castedBool.Should().Be(false);
        }
        [TestMethod]
        public void CastUnparsedJson_returning_null_given_object_is_of_JsonValueKind_Null_or_Undefined()
        {
            var castedNull = JsonElement.Parse("null").CastUnparsedJson();
            castedNull.Should().BeNull();
            var castedUndefined = new JsonElement().CastUnparsedJson();
            castedUndefined.Should().BeNull();
        }
        [TestMethod]
        public void CastUnparsedJson_returning_string_given_object_is_of_JsonValueKind_String()
        {
            var castedString = JsonElement.Parse("\"string\"").CastUnparsedJson();
            castedString.Should().Be("string");
        }
        [TestMethod]
        public void CastUnparsedJson_throwing_Exception_given_unsupported_JsonValueKind()
        {
            JsonElement.Parse("{}").Invoking(x => x.CastUnparsedJson()).Should().Throw<NotSupportedException>();
            JsonElement.Parse("[]").Invoking(x => x.CastUnparsedJson()).Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void CastOrThrow_returning_object_of_requested_Type_given_it_is_exposed()
        {
            IIdentifiable obj = new NamedIdentifiable();
            var supportedIface = obj.CastOrThrow<INamed>();
            supportedIface.Should().NotBeNull().And.BeAssignableTo<INamed>();
        }
        [TestMethod]
        public void CastOrThrow_throwing_Exception_given_requested_Type_is_not_exposed()
        {
            var obj = new Identifiable();
            obj.Invoking(x => x.CastOrThrow<INamed>()).Should().Throw<ArgumentException>();
        }
    }
}

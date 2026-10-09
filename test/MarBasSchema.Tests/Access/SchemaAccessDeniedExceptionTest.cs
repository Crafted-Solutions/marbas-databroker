using AwesomeAssertions;
using CraftedSolutions.MarBasSchema.Access;

namespace CraftedSolutions.MarBasSchema.Tests.Access
{
    [TestClass]
    public class SchemaAccessDeniedExceptionTest
    {
        [TestMethod]
        public void CTor_generating_Modifying_prohibited_message_given_Write_requestedAcces()
        {
            var exception = new SchemaAccessDeniedException(GrainAccessFlag.Write);
            exception.RequestedAcces.Should().Be(GrainAccessFlag.Write);
            exception.Message.Should().StartWith("Modifying of ");
        }

        [TestMethod]
        public void CTor_generating_Modifying_prohibited_message_given_WriteTraits_requestedAcces()
        {
            var exception = new SchemaAccessDeniedException(GrainAccessFlag.WriteTraits);
            exception.RequestedAcces.Should().Be(GrainAccessFlag.WriteTraits);
            exception.Message.Should().StartWith("Modifying of ");
        }

        [TestMethod]
        public void CTor_generating_Deleting_prohibited_message_given_Delete_requestedAcces()
        {
            var exception = new SchemaAccessDeniedException(GrainAccessFlag.Delete);
            exception.RequestedAcces.Should().Be(GrainAccessFlag.Delete);
            exception.Message.Should().StartWith("Deleting of ");
        }

        [TestMethod]
        public void CTor_generating_Creating_prohibited_message_given_CreateSubelement_requestedAcces()
        {
            var exception = new SchemaAccessDeniedException(GrainAccessFlag.CreateSubelement);
            exception.RequestedAcces.Should().Be(GrainAccessFlag.CreateSubelement);
            exception.Message.Should().StartWith("Creating ");
        }

        [TestMethod]
        public void CTor_generating_Access_prohibited_message_given_unknown_requestedAcces()
        {
            var exception = new SchemaAccessDeniedException(GrainAccessFlag.ModifyAcl);
            exception.RequestedAcces.Should().Be(GrainAccessFlag.ModifyAcl);
            exception.Message.Should().StartWith("Access to ");
        }
    }
}

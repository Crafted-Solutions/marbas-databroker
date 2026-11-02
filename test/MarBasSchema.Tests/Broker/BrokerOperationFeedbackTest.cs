using AwesomeAssertions;
using CraftedSolutions.MarBasSchema.Broker;
using Microsoft.Extensions.Logging;

namespace CraftedSolutions.MarBasSchema.Tests.Broker
{
    [TestClass]
    public class BrokerOperationFeedbackTest
    {
        [TestMethod]
        public void CTor_setting_Message()
        {
            var message = "test message";
            var feedback = new BrokerOperationFeedback(message);

            feedback.Message.Should().Be(message);
            feedback.Source.Should().Be("General");
            feedback.Code.Should().Be(0);
            feedback.ObjectId.Should().BeNull();
            feedback.FeedbackType.Should().Be(LogLevel.Information);
        }

        [TestMethod]
        public void CTor_setting_Message_and_Source()
        {
            var message = "test message";
            var source = "UnitTest";
            var feedback = new BrokerOperationFeedback(message, source);

            feedback.Message.Should().Be(message);
            feedback.Source.Should().Be(source);
            feedback.Code.Should().Be(0);
            feedback.ObjectId.Should().BeNull();
            feedback.FeedbackType.Should().Be(LogLevel.Information);
        }

        [TestMethod]
        public void CTor_setting_Message_Source_and_Code()
        {
            var message = "test message";
            var source = "UnitTest";
            var code = 42;
            var feedback = new BrokerOperationFeedback(message, source, code);

            feedback.Message.Should().Be(message);
            feedback.Source.Should().Be(source);
            feedback.Code.Should().Be(code);
            feedback.ObjectId.Should().BeNull();
            feedback.FeedbackType.Should().Be(LogLevel.Information);
        }

        [TestMethod]
        public void CTor_setting_Message_Source_Code_and_FeedbackType()
        {
            var message = "test message";
            var source = "UnitTest";
            var code = 42;
            var feedbackType = LogLevel.Warning;
            var feedback = new BrokerOperationFeedback(message, source, code, feedbackType);

            feedback.Message.Should().Be(message);
            feedback.Source.Should().Be(source);
            feedback.Code.Should().Be(code);
            feedback.ObjectId.Should().BeNull();
            feedback.FeedbackType.Should().Be(feedbackType);
        }

        [TestMethod]
        public void CTor_setting_Message_Source_Code_FeedbackType_and_ObjecId()
        {
            var message = "test message";
            var source = "UnitTest";
            var code = 42;
            var feedbackType = LogLevel.Warning;
            var id = Guid.NewGuid();
            var feedback = new BrokerOperationFeedback(message, source, code, feedbackType, id);

            feedback.Message.Should().Be(message);
            feedback.Source.Should().Be(source);
            feedback.Code.Should().Be(code);
            feedback.ObjectId.Should().Be(id);
            feedback.FeedbackType.Should().Be(feedbackType);
        }
    }
}

using AwesomeAssertions;
using CraftedSolutions.MarBasSchema.IO;

namespace CraftedSolutions.MarBasSchema.Tests.IO
{
    [TestClass]
    public class StreamableContentTest
    {
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        class EmptyContent : StreamableContent
        {
            public override byte[]? Data { get => null; set { } }
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataRow(0)]
        [DataRow(100)]
        [DataRow(4096)]
        public async Task CTor_setting_Data_Stream_and_Length(int dataLength)
        {
            var bytes = MakeBytes(dataLength);
            using var content = new StreamableContent(bytes);

            content.Length.Should().Be(null == bytes ? 0 : bytes.Length);
            if (null == bytes)
            {
                content.Data.Should().BeEmpty();
            }
            else
            {
                content.Data.Should().BeEquivalentTo(bytes);
            }

            using (var ins = content.Stream)
            {
                var strBytes = await StreamToBytes(ins, TestContext.CancellationToken);
                if (null == bytes)
                {
                    strBytes.Should().BeEmpty();
                }
                else
                {
                    strBytes.Should().BeEquivalentTo(bytes);
                }
            }
        }

        [TestMethod]
        [DataRow(100)]
        [DataRow(4096)]
        public async Task Set_Data_changing_Data_Length_and_Stream(int dataLength)
        {
            using var content = new StreamableContent();

            content.Length.Should().Be(0L);
            content.Data.Should().BeEmpty();

            var bytes = MakeBytes(dataLength)!;
            content.Data = bytes;
            
            content.Length.Should().Be(bytes.Length);
            content.Data.Should().BeEquivalentTo(bytes);

            using (var ins = content.Stream)
            {
                var strBytes = await StreamToBytes(ins, TestContext.CancellationToken);
                strBytes.Should().BeEquivalentTo(bytes);
            }
        }

        [TestMethod]
        [DataRow(100)]
        [DataRow(4096)]
        public async Task Set_Stream_changing_Stream_Length_and_Data(int dataLength)
        {
            using var content = new StreamableContent();

            content.Length.Should().Be(0L);
            content.Data.Should().BeEmpty();

            var bytes = MakeBytes(dataLength)!;
            using var dataStream = new BufferedStream(new MemoryStream(bytes));
            content.Stream = dataStream;

            content.Length.Should().Be(bytes.Length);
            content.Data.Should().BeEquivalentTo(bytes);

            using (var ins = content.Stream)
            {
                var strBytes = await StreamToBytes(ins, TestContext.CancellationToken);
                strBytes.Should().BeEquivalentTo(bytes);
            }

        }

        [TestMethod]
        public void Set_Stream_throwing_Exception_given_unreadable_Stream()
        {
            using var content = new StreamableContent();
            using var dataStream = new MemoryStream();
            dataStream.Close();

            content.Invoking(content => content.Stream = dataStream).Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void Get_Length_returning_zero_given_Data_is_null()
        {
            using var content = new EmptyContent();

            content.Length.Should().Be(0);
            content.Data.Should().BeNull();
        }

        private static byte[]? MakeBytes(int dataLength)
        {
            var result = 0 < dataLength ? new byte[dataLength] : null;
            if (null != result)
            {
                var rnd = new Random();
                rnd.NextBytes(result);
            }
            return result;
        }

        private static async Task<byte[]> StreamToBytes(Stream stream, CancellationToken cancellationToken)
        {
            using var os = new MemoryStream();
            await stream.CopyToAsync(os, cancellationToken);
            return os.ToArray();
        }
    }
}

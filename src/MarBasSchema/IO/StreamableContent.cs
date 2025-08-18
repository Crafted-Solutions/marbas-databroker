using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CraftedSolutions.MarBasSchema.IO
{
    public class StreamableContent(byte[]? data = null) : IStreamableContent
    {
        protected byte[]? _data = data;

        public virtual byte[]? Data
        {
            get
            {
                if (null != _data)
                {
                    return _data;
                }
                return ReadStream(Stream);
            }
            set => _data = value;
        }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual Stream Stream
        {
            get => null == _data ? new MemoryStream() : new MemoryStream(_data);
            set => _data = ReadStream(value);
        }

        public long Length => Data?.Length ?? Stream.Length;

        protected static byte[]? ReadStream(Stream s)
        {
            if (s.CanRead)
            {
                byte[]? result = null;
                if (s is MemoryStream memStream)
                {
                    result = memStream.ToArray();
                }
                else
                {
                    using (var ms = new MemoryStream())
                    {
                        s.CopyTo(ms);
                        result = ms.ToArray();
                    }
                    if (s.CanSeek)
                    {
                        s.Position = 0;
                    }
                }
                return result;
            }
            return null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // do nothing
        }
    }
}

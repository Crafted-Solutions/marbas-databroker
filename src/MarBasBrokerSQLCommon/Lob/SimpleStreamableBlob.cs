using System.Data.Common;
using MarBasSchema.IO;

namespace MarBasBrokerSQLCommon.Lob
{
    public sealed class SimpleStreamableBlob : StreamableContent, IAsyncStreamableContent
    {
        private readonly IBlobContext _context;
        private DbDataReader? _reader;
        private bool _disposed;
        private Stream? _stream;

        public SimpleStreamableBlob(IBlobContext context)
        {
            _context = context;
        }

        ~SimpleStreamableBlob() => Dispose(false);

        public override Stream Stream
        {
            get
            {
                return GetStreamAsync().Result;
            }
            set => base.Stream = value;
        }

        public async Task<Stream> GetStreamAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed || null != _data)
            {
                return base.Stream;
            }
            if (null != _stream)
            {
                return _stream;
            }
            _reader = await ExecuteQuery(cancellationToken);
            if (await _reader.ReadAsync(cancellationToken))
            {
                var ord = _reader.GetOrdinal(_context.DataColumn);
                if (-1 < ord && !await _reader.IsDBNullAsync(ord, cancellationToken))
                {
                    _stream = _reader.GetStream(ord);
                    return _stream;
                }
            }
            return base.Stream;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _reader?.Dispose();
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }

        private async Task<DbDataReader> ExecuteQuery(CancellationToken cancellationToken)
        {
            _reader?.Dispose();
            return await _context.Command.ExecuteReaderAsync(cancellationToken);
        }
    }
}

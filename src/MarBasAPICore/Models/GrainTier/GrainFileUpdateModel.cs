using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CraftedSolutions.MarBasSchema.GrainTier;
using CraftedSolutions.MarBasSchema.IO;
using Microsoft.AspNetCore.Http;

namespace CraftedSolutions.MarBasAPICore.Models.GrainTier
{
    public class GrainFileUpdateModel : IFileUploadModel
    {
        private IFormFile? _file;
        private readonly GrainFileWrapper _grain = new();

        [Required]
        public IFormFile File
        {
            get => _file!;
            set
            {
                _file = value;
                if (null != _file)
                {
                    if (!string.IsNullOrEmpty(_file.FileName))
                    {
                        _grain.Name = _file.FileName;
                    }
                    if (!string.IsNullOrEmpty(_file.ContentType))
                    {
                        _grain.MimeType = _file.ContentType;
                    }
                    _grain.Size = _file.Length;
                    _grain.Content = new ContentWrapper(_file.OpenReadStream());
                }
            }
        }

        public IGrainFile GetGrain(Guid id)
        {
            _grain.Id = id;
            return _grain;
        }

        private class GrainFileWrapper : GrainFile
        {
            public GrainFileWrapper(string? culture = null)
                : base(null, null, null, null == culture ? null : CultureInfo.GetCultureInfo(culture))
            {
            }

            public new Guid Id { get => base.Id; set => _props.Id = value; }
            public new CultureInfo CultureInfo { get => base.CultureInfo; set => _culture = value; }
        }

        private class ContentWrapper : StreamableContent
        {
            private Stream _stream;
            private bool _disposed;

            public ContentWrapper(Stream stream)
            {
                _stream = stream;
            }

            ~ContentWrapper() => Dispose(false);

            public override Stream Stream { get => _stream; set => _stream = value; }

            protected override void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        _stream?.Dispose();
                    }
                    _disposed = true;
                }
            }
        }
    }
}

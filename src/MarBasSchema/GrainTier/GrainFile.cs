using System.Globalization;
using System.Net.Mime;
using System.Security.Principal;
using MarBasCommon;
using MarBasSchema.Grain;
using MarBasSchema.IO;

namespace MarBasSchema.GrainTier
{
    public class GrainFile : GrainLocalized, IGrainFile
    {
        protected string _mimeType;
        protected long _size;
        protected IStreamableContent? _content;

        public GrainFile(Guid id, string? name, IIdentifiable? parent, IPrincipal? creator = null, CultureInfo? culture = null)
            : this(name, parent, creator, culture)
        {
            _props.Id = id;
        }

        public GrainFile(string? name, IIdentifiable? parent, IPrincipal? creator = null, CultureInfo? culture = null)
            : base(name, parent, creator, culture)
        {
            _mimeType = MediaTypeNames.Application.Octet;
            _content = null;
            _size = -1;
            _fieldTracker.AddScope<IGrainFile>();
        }

        public GrainFile(IGrainBase other)
            : base(other)
        {
            if (other is IGrainFile file)
            {
                _mimeType = file.MimeType;
                _content = file.Content;
                _size = file.Size;
            }
            else
            {
                _mimeType = MediaTypeNames.Application.Octet;
                _content = null;
                _size = 0;
            }
            _fieldTracker.AddScope<IGrainFile>();
        }

        public string MimeType
        {
            get => _mimeType;
            set
            {
                if (value != _mimeType)
                {
                    _mimeType = value;
                    _fieldTracker.TrackPropertyChange<IGrainFile>();
                }
            }
        }

        public long Size
        {
            get => -1 < _size ? _size : (_size = _content?.Length ?? 0);
            set
            {
                if (null == _content && value != _size)
                {
                    _size = value;
                    _fieldTracker.TrackPropertyChange<IGrainFile>();
                }
            }
        }

        public IStreamableContent? Content
        {
            get => _content;
            set
            {
                var oldSize = Size;
                _content = value;
                _fieldTracker.TrackPropertyChange<IGrainFile>();
                var newSize = _content?.Length ?? 0;
                if (newSize != oldSize)
                {
                    _size = newSize;
                    if (-1 < oldSize)
                    {
                        _fieldTracker.TrackPropertyChange<IGrainFile>(nameof(Size));
                    }
                }
            }
        }
    }
}

using System.Globalization;
using System.Net.Mime;
using System.Security.Principal;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;
using CraftedSolutions.MarBasSchema.IO;

namespace CraftedSolutions.MarBasSchema.GrainTier
{
    public class GrainFile : GrainLocalized, IGrainFile
    {
        protected static readonly IGrain DefaultType = new GrainPlain()
        {
            Id = SchemaDefaults.FileTypeDefID,
            Name = SchemaDefaults.FileTypeName,
        };

        protected string _mimeType;
        protected long _size;
        protected IStreamableContent? _content;

        // TODO remove this constructor, GrainTransportBroker should use its own subclass
        [Obsolete("Do not use, declare own subclass to initalize Id")]
        public GrainFile(Guid id, string? name, IIdentifiable? parent, IPrincipal? creator = null, CultureInfo? culture = null)
            : this(name, parent, creator, culture)
        {
            _props.Id = id;
        }

        public GrainFile(string? name = null, IIdentifiable? parent = null, IPrincipal? creator = null, CultureInfo? culture = null)
            : base(name, parent, creator, culture)
        {
            _typeConstraint = new SimpleTypeConstraint(DefaultType);
            _mimeType = MediaTypeNames.Application.Octet;
            _content = null;
            _size = -1;
            _fieldTracker.AddScope<IGrainFile>();
        }

        public GrainFile(IGrain other)
            : base(other)
        {
            if (null == _typeConstraint)
            {
                _typeConstraint = new SimpleTypeConstraint(DefaultType);
            }
            if (other is IFile file)
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
                var newValue = string.IsNullOrEmpty(value) ? MediaTypeNames.Application.Octet : value;
                if (_fieldTracker.IsChangeAccepted(_mimeType, newValue))
                {
                    _mimeType = newValue;
                    _fieldTracker.TrackPropertyChange<IGrainFile>();
                }
            }
        }

        public long Size
        {
            get => -1 < _size ? _size : (_size = _content?.Length ?? 0);
            set
            {
                if (null == _content && _fieldTracker.IsChangeAccepted(_size, value))
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
                if (!_fieldTracker.AcceptAllChanges && ((null == _content && null == value) || ReferenceEquals(_content, value)))
                {
                    return;
                }
                var oldSize = Size;
                _content = value;
                _fieldTracker.TrackPropertyChange<IGrainFile>();
                var newSize = _content?.Length ?? 0;
                if (_fieldTracker.IsChangeAccepted(oldSize, newSize))
                {
                    _size = newSize;
                    _fieldTracker.TrackPropertyChange<IGrainFile>(nameof(Size));
                }
            }
        }
    }
}

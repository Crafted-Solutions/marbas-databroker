using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Net.Mime;
using MarBasBrokerSQLCommon.Grain;
using MarBasSchema.GrainTier;
using MarBasSchema.IO;

namespace MarBasBrokerSQLCommon.GrainTier
{
    public class GrainFileInlineDataAdapter : GrainLocalizedDataAdapter, IGrainFile
    {
        protected readonly GrainFileContentAccess _loadContent;

        public GrainFileInlineDataAdapter(DbDataReader dataReader, GrainFileContentAccess loadContent = GrainFileContentAccess.OnDemand) : base(dataReader)
        {
            _loadContent = loadContent;
        }

        [Column(GrainFileDefaults.FieldMimeType)]
        public string MimeType { get => GetNullableField(GetMappedColumnName(), MediaTypeNames.Application.Octet)!; set => throw new NotImplementedException(); }

        public long Size => GetNullableField(GetMappedColumnName(), 0L);

        public virtual IStreamableContent? Content
        {
            get
            {
                if (GrainFileContentAccess.None != _loadContent)
                {
                    var ord = _dataReader.GetOrdinal(GetMappedColumnName());
                    if (-1 < ord && !_dataReader.IsDBNull(ord))
                    {
                        var buff = new byte[Size];
                        _dataReader.GetBytes(ord, 0, buff, 0, buff.Length);
                        return new StreamableContent(buff);
                    }
                }
                return null;
            }
            set => throw new NotImplementedException();
        }
    }
}

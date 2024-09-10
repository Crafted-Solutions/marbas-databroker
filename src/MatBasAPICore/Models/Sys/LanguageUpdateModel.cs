using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MarBasSchema.Sys;

namespace MarBasAPICore.Models.Sys
{
    public sealed class LanguageUpdateModel
    {
        private SystemLanguageWrapper _lanugage = new ();

        public string IsoCode { get => _lanugage.IsoCode; set => _lanugage.IsoCode = value; }
        public string Label { get => _lanugage.Label; set => _lanugage.Label = value; }
        public string? LabelNative { get => _lanugage.LabelNative; set => _lanugage.LabelNative = value; }

        [JsonIgnore]
        [IgnoreDataMember]
        public ISystemLanguage Language => _lanugage;

        private class SystemLanguageWrapper : SystemLanguage
        {
            public SystemLanguageWrapper() : base(CultureInfo.InvariantCulture)
            {
                _fieldTracker.AcceptAllChanges = true;
            }
        }
    }
}

using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon;

namespace CraftedSolutions.MarBasSchema.Grain
{
    public class TraitRef(IIdentifiable grain, IIdentifiable propdef, CultureInfo? culture = null) : ITraitRef
    {
        protected IIdentifiable _grain = grain;
        protected IIdentifiable _propdef = propdef;
        protected CultureInfo? _culture = culture;
        protected int _revision = 1;

        [JsonIgnore]
        [IgnoreDataMember]
        public IIdentifiable Grain { get => _grain; set => _grain = value; }

        public Guid GrainId => Grain.Id;

        [JsonIgnore]
        [IgnoreDataMember]
        public IIdentifiable PropDef { get => _propdef; set => _propdef = value; }

        public Guid PropDefId => PropDef.Id;

        public int Revision { get => _revision; set => _revision = value; }

        [JsonIgnore]
        [IgnoreDataMember]
        public CultureInfo? CultureInfo { get => _culture; set => _culture = value; }

        public string? Culture => CultureInfo?.IetfLanguageTag;
    }
}

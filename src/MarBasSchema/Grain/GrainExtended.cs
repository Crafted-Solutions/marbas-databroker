using System.Security.Principal;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasCommon.Json;
using CraftedSolutions.MarBasSchema.Access;

namespace CraftedSolutions.MarBasSchema.Grain
{
    public class GrainExtended : GrainBase, IGrainExtended
    {
        protected GrainAccessFlag _permissions;
        protected string? _typeXAttrs;
        protected int _childCount;

        public GrainExtended(IGrainBase other)
            : base(other)
        {
            if (other is IGrainExtended grainExtended)
            {
                _permissions = grainExtended.Permissions;
                _childCount = grainExtended.ChildCount;
                _typeXAttrs = grainExtended.TypeXAttrs;
            }
        }

        public GrainExtended(string? name = null, IIdentifiable? parent = null, IPrincipal? creator = null)
            : base(name, parent, creator)
        {
        }

        internal GrainExtended(Guid id, string? name = null, IIdentifiable? parent = null, IPrincipal? creator = null)
            : base(id, name, parent, creator)
        {
        }

        public string? TypeXAttrs => _typeXAttrs;

        [JsonConverter(typeof(RawValueJsonConverter<GrainAccessFlag>))]
        public GrainAccessFlag Permissions => _permissions;

        public int ChildCount => _childCount;
    }
}

using System.Globalization;
using CraftedSolutions.MarBasAPICore.Models.Grain;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.GrainDef;

namespace CraftedSolutions.MarBasAPICore.Models.GrainDef
{
    public class TypeDefUpdateModel : GenericGrainUpdateModel<IGrainTypeDef, TypeDefUpdateModel.TypeDefWrapper>, ITypeDefUpdateModel
    {
        public string? Impl { get => _grain.Impl; set => _grain.Impl = value; }
        public IEnumerable<Guid>? MixInIds
        {
            get => _grain.MixIns.Select(i => i.Id);
            set
            {
                _grain.ReplaceMixIns(value?.Select(x => (Identifiable)x));
            }
        }

        public class TypeDefWrapper : GrainTypeDef, IUpdateableGrain
        {
            public TypeDefWrapper()
                : base(null, null, null, null, null)
            {
                _fieldTracker.AcceptAllChanges = true;
            }

            Guid IUpdateableGrain.Id { get => Id; set => _props.Id = value; }
            CultureInfo IUpdateableGrain.CultureInfo { get => CultureInfo; set => _culture = value; }
        }
    }
}

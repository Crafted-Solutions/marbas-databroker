using System.Globalization;
using MarBasSchema.Grain;

namespace MarBasAPICore.Models.Grain
{
    public sealed class GrainUpdateModel : GenericGrainUpdateModel<IGrainLocalized, GrainUpdateModel.GrainWrapper>
    {
        public class GrainWrapper : GrainLocalized, IUpdateableGrain
        {
            public GrainWrapper()
                : base(null, null, null, null)
            {
                _fieldTracker.AcceptAllChanges = true;
            }

            Guid IUpdateableGrain.Id { get => Id; set => _props.Id = value; }
            CultureInfo IUpdateableGrain.CultureInfo { get => CultureInfo; set => _culture = value; }
        }
    }
}

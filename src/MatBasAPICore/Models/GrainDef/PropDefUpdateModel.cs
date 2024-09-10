using System.Globalization;
using MarBasAPICore.Models.Grain;
using MarBasCommon;
using MarBasSchema;
using MarBasSchema.GrainDef;

namespace MarBasAPICore.Models.GrainDef
{
    public class PropDefUpdateModel : GenericGrainUpdateModel<IGrainPropDef, PropDefUpdateModel.PropDefWrapper>, IPropDefUpdateModel
    {
        public TraitValueType? ValueType
        {
            get => _grain.ValueType;
            set => _grain.ValueType = value ?? TraitValueType.Text;
        }

        public Guid? ValueConstraintId
        {
            get => _grain.ValueConstraint?.Id;
            set => _grain.ValueConstraint = (Identifiable)value!;
        }

        public string? ConstraintParams
        {
            get => _grain.ConstraintParams;
            set => _grain.ConstraintParams = value;
        }

        public int? CardinalityMin
        {
            get => _grain.CardinalityMin;
            set => _grain.CardinalityMin = value ?? 1;
        }

        public int? CardinalityMax
        {
            get => _grain.CardinalityMax;
            set => _grain.CardinalityMax = value ?? 1;
        }

        public bool? Versionable
        {
            get => _grain.Versionable;
            set => _grain.Versionable = value ?? true;
        }

        public bool? Localizable
        {
            get => _grain.Localizable;
            set => _grain.Localizable = value ?? true;
        }

        public class PropDefWrapper : GrainPropDef, IUpdateableGrain
        {
            public PropDefWrapper()
            {
                _fieldTracker.AcceptAllChanges = true;
            }

            Guid IUpdateableGrain.Id { get => Id; set => _props.Id = value; }
            CultureInfo IUpdateableGrain.CultureInfo { get => CultureInfo; set => _culture = value; }
        }
    }
}

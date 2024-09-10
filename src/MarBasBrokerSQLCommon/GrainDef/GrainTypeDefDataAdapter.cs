using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using MarBasBrokerSQLCommon.Grain;
using MarBasCommon;
using MarBasSchema.GrainDef;

namespace MarBasBrokerSQLCommon.GrainDef
{
    public class GrainTypeDefDataAdapter : GrainLocalizedDataAdapter, IGrainTypeDef
    {
        protected ISet<IIdentifiable> _mixins;

        public GrainTypeDefDataAdapter(DbDataReader dataReader) : base(dataReader)
        {
            _mixins = new HashSet<IIdentifiable>();
        }

        public string? Impl { get => GetNullableField<string>(GetMappedColumnName()); set => throw new NotImplementedException(); }

        [Column(GrainTypeDefDefaults.FieldDefaultInstance)]
        public Guid? DefaultInstanceId => DefaultInstance?.Id;
        [Column(GrainTypeDefDefaults.FieldDefaultInstance)]
        public IIdentifiable? DefaultInstance { get => (Identifiable?)GetNullableGuid(GetMappedColumnName()); set => throw new NotImplementedException(); }

        [Column(GrainTypeDefDefaults.MixinExtFieldBaseType)]
        public IEnumerable<IIdentifiable> MixIns { get => _mixins; set => _mixins = value.ToHashSet(); }

        public IEnumerable<Guid> MixInIds => _mixins.Select(x => x.Id);

        public void AddMixIn(IIdentifiable typeDef) => _mixins.Add(typeDef);

        public void ClearMixIns() => _mixins.Clear();

        public void RemoveMixIn(IIdentifiable typeDef) => _mixins.Remove(typeDef);

        public void ReplaceMixIns(IEnumerable<IIdentifiable>? mixins)
        {
            if (null == mixins)
            {
                _mixins.Clear();
            }
            else
            {
                _mixins = new HashSet<IIdentifiable>(mixins);
            }
        }
    }
}

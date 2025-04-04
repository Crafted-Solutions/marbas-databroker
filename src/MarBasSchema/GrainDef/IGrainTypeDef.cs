﻿using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasSchema.GrainDef
{
    public interface IGrainTypeDef : IGrainBase, ITypeDef
    {
        Guid? DefaultInstanceId { get; }
        [JsonIgnore]
        [IgnoreDataMember]
        IIdentifiable? DefaultInstance { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        IEnumerable<IIdentifiable> MixIns { get; }
        void AddMixIn(IIdentifiable typeDef);
        void RemoveMixIn(IIdentifiable typeDef);
        void ReplaceMixIns(IEnumerable<IIdentifiable>? mixins);
        void ClearMixIns();
    }

    public interface IGrainTypeDefLocalized : IGrainTypeDef, IGrainLocalized
    {
    }
}

﻿using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon;

namespace CraftedSolutions.MarBasSchema.Grain
{
    public interface ITraitRef : IGrainBinding, ILocalizable
    {
        [ReadOnly(true)]
        [JsonIgnore]
        [IgnoreDataMember]
        IIdentifiable PropDef { get; set; }
        Guid PropDefId { get; }
        int Revision { get; set; }
    }
}

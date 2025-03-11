using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasCommon;

namespace CraftedSolutions.MarBasAPICore.Models.Grain
{
    public interface IGrainUpdateModel<TGrain> : IIdentifiable
    {
        string? Name { get; set; }
        string? Label { get; set; }
        string? Culture { get; set; }
        string? SortKey { get; set; }
        string? XAttrs { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        TGrain Grain { get; }
    }
}

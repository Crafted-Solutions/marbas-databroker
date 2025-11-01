using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CraftedSolutions.MarBasCommon.Job
{

    public interface IBackgroudJobContext : IBackgroundJobState
    {
        [JsonIgnore]
        [IgnoreDataMember]
        CancellationToken CancellationToken { get; }
    }
}

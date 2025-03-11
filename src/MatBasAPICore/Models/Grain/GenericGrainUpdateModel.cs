using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasAPICore.Models.Grain
{
    public class GenericGrainUpdateModel<TGrain, TGrainImpl>
        : IGrainUpdateModel<TGrain> where TGrain : IGrainBase where TGrainImpl : TGrain, GenericGrainUpdateModel<TGrain, TGrainImpl>.IUpdateableGrain, new()
    {
        protected readonly TGrainImpl _grain = new();

        [Required]
        public Guid Id { get => _grain.Id; set => _grain.Id = value; }
        public string? Name { get => _grain.Name; set => _grain.Name = value!; }
        public string? Label { get => _grain.Label; set => _grain.Label = value; }
        public string? Culture { get => _grain.Culture; set => _grain.CultureInfo = (null == value ? null : CultureInfo.GetCultureInfo(value))!; }
        public string? SortKey { get => _grain.SortKey; set => _grain.SortKey = value; }
        public string? XAttrs { get => _grain.XAttrs; set => _grain.XAttrs = value; }
        public int? CustomFlag { get => _grain.CustomFlag; set => _grain.CustomFlag = value ?? 0; }

        [JsonIgnore]
        [IgnoreDataMember]
        public TGrain Grain => _grain;

        public interface IUpdateableGrain : IGrainLocalized
        {
            new Guid Id { get; set; }
            new string Name { get; set; }
            new CultureInfo CultureInfo { get; set; }
        }
    }
}

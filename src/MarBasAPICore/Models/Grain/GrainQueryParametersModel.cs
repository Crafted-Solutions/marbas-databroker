using CraftedSolutions.MarBasAPICore.Swagger;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Broker;
using Microsoft.AspNetCore.Mvc;

namespace CraftedSolutions.MarBasAPICore.Models.Grain
{
    public sealed class GrainQueryParametersModel
    {
        /// <summary>
        /// List of type GUIDs (use "00000000-0000-0000-0000-000000000000" or "Type" for type of TypeDef itself) or type names
        /// </summary>
        [ModelBinder(BinderType = typeof(SwaggerEnumerable<string[]>), Name = "typeFilter")]
        public IEnumerable<string>? TypeFilter { get; set; }

        [ModelBinder(BinderType = typeof(SwaggerEnumerable<Guid[]>), Name = "idFilter")]
        public IEnumerable<Guid>? IdFilter { get; set; }

        [ModelBinder(Name = "mTimeFrom")]
        public DateTime? MTimeFrom { get; set; }

        [ModelBinder(Name = "mTimeTo")]
        public DateTime? MTimeTo { get; set; }

        [ModelBinder(Name = "mTimeIncluding")]
        public RangeInclusionFlag? MTimeIncluding { get; set; }

        /// <summary>
        /// List of Grain fields to sort results by
        /// </summary>
        [ModelBinder(BinderType = typeof(SwaggerEnumerable<ListSortOption<GrainSortField>[]>), Name = "sortOptions")]
        public IEnumerable<ListSortOption<GrainSortField>>? SortOptions { get; set; }

        public IGrainQueryFilter? ToQueryFilter()
        {
            if (null != TypeFilter?.FirstOrDefault() || default != IdFilter?.FirstOrDefault() || null != MTimeFrom || null != MTimeTo)
            {
                var result = new GrainQueryFilter()
                {
                    TypeConstraints = GetTypeConstraints(TypeFilter),
                    IdConstraints = IdFilter
                };
                if (null != MTimeFrom || null != MTimeTo)
                {
                    result.MTimeConstraint = new SimpleTimeRangeConstraint()
                    {
                        Start = MTimeFrom,
                        End = MTimeTo,
                        Including = MTimeIncluding ?? RangeInclusionFlag.None
                    };
                }
                return result;
            }
            return null;
        }

        public static IEnumerable<ITypeConstraint>? GetTypeConstraints(IEnumerable<string>? types) => types?.Select((x) =>
        {
            if (!string.IsNullOrEmpty(x) && Guid.TryParse(x, out var id))
            {
                return new SimpleTypeConstraint(id);
            }
            return new SimpleTypeConstraint(Guid.Empty, x);
        });
    }
}

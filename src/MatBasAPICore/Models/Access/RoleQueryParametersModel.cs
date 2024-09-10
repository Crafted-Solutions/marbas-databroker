using MarBasAPICore.Swagger;
using MarBasSchema.Broker;
using Microsoft.AspNetCore.Mvc;

namespace MarBasAPICore.Models.Access
{
    public sealed class RoleQueryParametersModel
    {
        [ModelBinder(BinderType = typeof(SwaggerEnumerable<ListSortOption<RoleSortField>[]>), Name = "sortOptions")]
        public IEnumerable<ListSortOption<RoleSortField>>? SortOptions { get; set; }
    }
}

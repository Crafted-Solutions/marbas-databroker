using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Access;

namespace CraftedSolutions.MarBasSchema.Broker
{
    public interface IAsyncRoleManagementBroker
    {
        Task<ISchemaRole?> GetRoleAsync(Guid id, CancellationToken cancellationToken = default);
        Task<int> DeleteRolesAsync(IEnumerable<IIdentifiable> ids, CancellationToken cancellationToken = default);
        Task<int> StoreRolesAsync(IEnumerable<ISchemaRole> roles, CancellationToken cancellationToken = default);
        Task<ISchemaRole?> CreateRoleAsync(string name, RoleEntitlement entitlement = RoleEntitlement.None, CancellationToken cancellationToken = default);
        Task<IEnumerable<ISchemaRole>> ListRolesAsync(IEnumerable<IListSortOption<RoleSortField>>? sortOptions = null, CancellationToken cancellationToken = default);
    }
}

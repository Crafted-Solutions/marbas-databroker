using MarBasCommon;
using MarBasSchema.Access;

namespace MarBasSchema.Broker
{
    public interface IRoleManagementBroker
    {
        ISchemaRole? GetRole(Guid id);
        int DeleteRoles(IEnumerable<IIdentifiable> ids);
        int StoreRoles(IEnumerable<ISchemaRole> roles);
        ISchemaRole? CreateRole(string name, RoleEntitlement entitlement = RoleEntitlement.None);
        IEnumerable<ISchemaRole> ListRoles(IEnumerable<IListSortOption<RoleSortField>>? sortOptions = null);
    }
}

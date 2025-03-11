using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Access;

namespace CraftedSolutions.MarBasSchema.Broker
{
    public interface IAsyncAclManagementBroker
    {
        Task<ISchemaAclEntry?> GetAclEntryAsync(IIdentifiable role, IIdentifiable grain, CancellationToken cancellationToken = default);
        Task<ISchemaAclEntry?> CreateAclEntryAsync(IIdentifiable role, IIdentifiable grain, GrainAccessFlag permissionMask = GrainAccessFlag.Read, GrainAccessFlag restrictionMask = GrainAccessFlag.None, bool inherit = true, CancellationToken cancellationToken = default);
        Task<int> StoreAclAsync(IEnumerable<ISchemaAclEntry> acl, CancellationToken cancellationToken = default);
        Task<int> DeleteAclAsync(IEnumerable<IAclEntryRef> acl, CancellationToken cancellationToken = default);
        Task<IEnumerable<ISchemaAclEntry>> GetEffectiveAclAsync(IIdentifiable grain, CancellationToken cancellationToken = default);
    }
}

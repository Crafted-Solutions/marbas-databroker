using MarBasCommon;
using MarBasSchema.Access;

namespace MarBasSchema.Broker
{
    public interface IAclManagementBroker
    {
        ISchemaAclEntry? GetAclEntry(IIdentifiable role, IIdentifiable grain);
        ISchemaAclEntry? CreateAclEntry(IIdentifiable role, IIdentifiable grain, GrainAccessFlag permissionMask = GrainAccessFlag.Read, GrainAccessFlag restrictionMask = GrainAccessFlag.None, bool inherit = true);
        int StoreAcl(IEnumerable<ISchemaAclEntry> acl);
        int DeleteAcl(IEnumerable<IAclEntryRef> acl);
        IEnumerable<ISchemaAclEntry> GetEffectiveAcl(IIdentifiable grain);
    }
}

using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Event;

namespace CraftedSolutions.MarBasSchema.Broker
{
    public interface IBrokerProfile
    {
        bool IsOnline { get; }
        Version Version { get; }
        Guid InstanceId { get; }
        IEnumerable<ISchemaRole> SchemaRoles { get; }

        event EventHandler<SchemaModifiedEventArgs<IIdentifiable>>? SchemaModified;
        void DispatchSchemaModified<TSubject>(SchemaModificationType changeType, IEnumerable<IIdentifiable> subjects);
    }
}

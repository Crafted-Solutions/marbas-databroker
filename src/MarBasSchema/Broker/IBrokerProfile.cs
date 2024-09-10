using MarBasCommon;
using MarBasSchema.Access;
using MarBasSchema.Event;

namespace MarBasSchema.Broker
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

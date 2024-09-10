namespace MarBasSchema.Sys
{
    public interface IServerInfo
    {
        string Name { get; }
        string Description { get; }
        Version Version { get; }
        Version? SchemaVersion { get; set; }
        Guid InstanceId { get; set; }
    }
}

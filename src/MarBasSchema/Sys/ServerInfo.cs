namespace CraftedSolutions.MarBasSchema.Sys
{
    public class ServerInfo : IServerInfo
    {
        public string Name => "MarBas";
        public string Description => "MarBas holds the keys to the secrets...";
        public Version Version => System.Reflection.Assembly.GetAssembly(GetType())?.GetName().Version ?? new Version(0, 1, 0);
        public Version? SchemaVersion { get; set; }
        public Guid InstanceId { get; set; } = Guid.Empty;
    }
}

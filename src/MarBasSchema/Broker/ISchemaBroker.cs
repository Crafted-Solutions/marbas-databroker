namespace MarBasSchema.Broker
{
    public interface ISchemaBroker:
        IProfileProvider, ICloningBroker,
        ISystemLanguageBroker, IGrainManagementBroker,
        ITraitManagementBroker, IFileManagementBroker,
        IGrainDefManagementBroker, IGrainTransportBroker
    {
    }
}

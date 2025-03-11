namespace CraftedSolutions.MarBasSchema.Broker
{
    public interface IAsyncSchemaBroker :
        IProfileProvider, IAsyncCloningBroker,
        IAsyncSystemLanguageBroker, IAsyncGrainManagementBroker,
        IAsyncTraitManagementBroker, IAsyncFileManagementBroker,
        IAsyncGrainDefManagementBroker, IAsyncGrainTransportBroker
    {
    }
}

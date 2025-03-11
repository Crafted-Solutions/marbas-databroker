namespace CraftedSolutions.MarBasCommon.DependencyInjection
{
    public interface IInitializerService
    {
        IInitializerService AddInitService(Type serviceType);
        IInitializerService AddInitService<TService>();
        IInitializerService AddMultipleInitServices(IEnumerable<Type> serviceTypes);
        Task InitializeServicesAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
    }
}

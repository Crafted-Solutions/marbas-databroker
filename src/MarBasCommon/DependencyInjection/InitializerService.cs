namespace CraftedSolutions.MarBasCommon.DependencyInjection
{
    public class InitializerService : IInitializerService
    {
        protected readonly ISet<Type> _initServices;
        protected readonly SemaphoreSlim _semaphore = new(1, 1);

        public InitializerService()
        {
            _initServices = new HashSet<Type>();
        }

        public IInitializerService AddInitService<TService>()
        {
            return AddInitService(typeof(TService));
        }

        public IInitializerService AddInitService(Type serviceType)
        {
            _initServices.Add(serviceType);
            return this;
        }

        public IInitializerService AddMultipleInitServices(IEnumerable<Type> serviceTypes)
        {
            foreach (var serviceType in serviceTypes)
            {
                _initServices.Add(serviceType);
            }
            return this;
        }

        public async Task InitializeServicesAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                foreach (var type in _initServices)
                {
                    var service = serviceProvider.GetService(type);
                    if (service is IAsyncInitService asyncInit)
                    {
                        await asyncInit.InitServiceAsync(cancellationToken);
                    }
                }
                _initServices.Clear();

            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}

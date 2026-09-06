using AwesomeAssertions;
using CraftedSolutions.MarBasCommon.DependencyInjection;

namespace CraftedSolutions.MarBasCommon.Tests.DependencyInjection
{
    [TestClass]
    public class InitializerServiceTest
    {
        internal class FirstInitService : IAsyncInitService
        {
            public bool Initalized { get; set; }
            public Task<bool> InitServiceAsync(CancellationToken cancellationToken = default)
            {
                Initalized = true;
                return Task.FromResult(true);
            }
        }
        internal class SecondInitService : IAsyncInitService
        {
            public bool Initalized { get; set; }
            public Task<bool> InitServiceAsync(CancellationToken cancellationToken = default)
            {
                Initalized = true;
                return Task.FromResult(true);
            }
        }
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal class UselessService { }
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal class ServiceProviderMock : IServiceProvider
        {
            public FirstInitService FirstService = new();
            public SecondInitService SecondService = new();

            public object? GetService(Type serviceType) => serviceType switch
            {
                var type when type == typeof(FirstInitService) => FirstService,
                var type when type == typeof(SecondInitService) => SecondService,
                var type when type == typeof(UselessService) => new UselessService(),
                _ => null
            };
        }

        private readonly ServiceProviderMock _serviceProvider = new();

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void AddInitService_adding_service_Type_once()
        {
            var initializer = new InitializerService();
            initializer.AddInitService<FirstInitService>();
            initializer.InitServices.Should().NotBeEmpty().And.Satisfy(
                type => typeof(FirstInitService) == type
                );
            initializer.AddInitService<FirstInitService>();
            initializer.InitServices.Should().HaveCount(1).And.Satisfy(
                type => typeof(FirstInitService) == type
                ); ;
        }
        [TestMethod]
        public void AddInitService_adding_different_service_Types()
        {
            var initializer = new InitializerService();
            initializer
                .AddInitService<FirstInitService>()
                .AddInitService<SecondInitService>();
            initializer.InitServices.Should().NotBeEmpty().And.Satisfy(
                type => typeof(FirstInitService) == type,
                type => typeof(SecondInitService) == type
                );
        }
        
        [TestMethod]
        public void AddMultipleInitServices_adding_service_Types_excluding_doublettes()
        {
            var initializer = new InitializerService();
            initializer.AddMultipleInitServices([
                typeof(FirstInitService),
                typeof(SecondInitService),
                typeof(FirstInitService),
                typeof(UselessService)]);
            initializer.InitServices.Should().NotBeEmpty().And.Satisfy(
                type => typeof(FirstInitService) == type,
                type => typeof(SecondInitService) == type,
                type => typeof(UselessService) == type
                );
        }

        [TestMethod]
        public async Task InitializeServicesAsync_calling_InitServiceAsync_on_all_available_IAsyncInitServices()
        {
            using var initializer = new InitializerService();
            initializer
                .AddInitService<FirstInitService>()
                .AddInitService<SecondInitService>()
                .AddInitService<UselessService>();
            await initializer.InitializeServicesAsync(_serviceProvider, TestContext.CancellationToken);
            initializer.InitServices.Should().BeEmpty();
            _serviceProvider.FirstService.Initalized.Should().BeTrue();
            _serviceProvider.SecondService.Initalized.Should().BeTrue();
            // double dispose: branch coverage
            initializer.Dispose();
        }

        [TestMethod]
        public async Task InitializeServicesAsync_throwing_Exception_given_cancelled_CancellationToken()
        {
            using var initializer = new InitializerService();
            initializer.AddInitService<FirstInitService>();

            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();
            await initializer.Invoking(x => x.InitializeServicesAsync(_serviceProvider, tokenSource.Token)).Should().ThrowAsync<TaskCanceledException>();
        }
    }
}

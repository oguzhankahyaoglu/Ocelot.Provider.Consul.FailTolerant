using Ocelot.ServiceDiscovery.Providers;

namespace Ocelot.Provider.Consul
{
    using System.Threading.Tasks;
    using Logging;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceDiscovery;

    public static class ConsulProviderFactory
    {
        public static ServiceDiscoveryFinderDelegate Get = (provider, config, route) =>
        {
            // IOcelotLoggerFactory service1 = provider.GetService<IOcelotLoggerFactory>();
            // IConsulClientFactory service2 = provider.GetService<IConsulClientFactory>();
            // Consul consulServiceDiscoveryProvider = 
            //     new Consul(
            //         new ConsulRegistryConfiguration(config.Scheme, config.Host, config.Port, route.ServiceName, config.Token), 
            //         service1, service2);
            // return config.Type?.ToLower() == "pollconsul" 
            //     ? new PollConsul(config.PollingInterval, service1, 
            //         consulServiceDiscoveryProvider) 
            //     : consulServiceDiscoveryProvider;


            var factory = provider.GetService<IOcelotLoggerFactory>();

            var consulFactory = provider.GetService<IConsulClientFactory>();

            var consulRegistryConfiguration = new ConsulRegistryConfiguration(config.Scheme, config.Host, config.Port,
                route.ServiceName, config.Token);

            var consulServiceDiscoveryProvider = new Consul(consulRegistryConfiguration, factory, consulFactory);

            if (config.Type?.ToLower() == "pollconsul")
            {
                return new PollConsul(config.PollingInterval, factory, consulServiceDiscoveryProvider);
            }

            return consulServiceDiscoveryProvider;
        };
    }
}

using Microsoft.Extensions.DependencyInjection.Extensions;
using Ocelot.Configuration.Creator;

namespace Ocelot.Provider.Consul
{
    using Configuration.Repository;
    using DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Middleware;
    using ServiceDiscovery;

    public static class OcelotBuilderExtensions
    {
        public static IOcelotBuilder AddConsul(this IOcelotBuilder builder)
        {
            builder.Services.AddSingleton(ConsulProviderFactory.Get);
            builder.Services.AddSingleton<IConsulClientFactory, ConsulClientFactory>();
            return builder;
        }

        public static IOcelotBuilder AddConfigStoredInConsul(this IOcelotBuilder builder)
        {
            var services = builder.Services;
            services.AddSingleton(ConsulMiddlewareConfigurationProvider.Get);
            services.AddHostedService<FileConfigurationPoller>();

            services.RemoveAll(typeof(IFileConfigurationRepository));
            services.AddSingleton<IFileConfigurationRepository, ConsulConfigurationRepository>();

            services.RemoveAll(typeof(IFileConfigurationPollerOptions));
            services.AddTransient<IFileConfigurationPollerOptions, ConsulFileConfigurationPollerOptionExtended>();

            services.RemoveAll(typeof(IInternalConfigurationCreator));
            services.AddTransient<IInternalConfigurationCreator, FileInternalConfigurationCreatorExtended>();
            
            var assembly = typeof(OcelotBuilderExtensions).Assembly;
            services.AddMvc().AddApplicationPart(assembly);
            return builder;
        }
    }
}

﻿using Newtonsoft.Json;

namespace Ocelot.Provider.Consul.UnitTests
{
    using System;
    using Configuration;
    using Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Shouldly;
    using Xunit;

    public class ProviderFactoryTests
    {
        private readonly IServiceProvider _provider;

        public ProviderFactoryTests()
        {
            var services = new ServiceCollection();
            var loggerFactory = new Mock<IOcelotLoggerFactory>();
            var logger = new Mock<IOcelotLogger>();
            loggerFactory.Setup(x => x.CreateLogger<Consul>()).Returns(logger.Object);
            loggerFactory.Setup(x => x.CreateLogger<PollConsul>()).Returns(logger.Object);
            var consulFactory = new Mock<IConsulClientFactory>();
            services.AddSingleton<IConsulClientFactory>(consulFactory.Object);
            services.AddSingleton<IOcelotLoggerFactory>(loggerFactory.Object);
            _provider = services.BuildServiceProvider();
        }

        [Fact]
        public void should_return_ConsulServiceDiscoveryProvider()
        {
            var provider = ConsulProviderFactory.Get(_provider, new ServiceProviderConfiguration("", "","", 1, "", "", 1), 
                JsonConvert.DeserializeObject<DownstreamRoute>("{}"));
            provider.ShouldBeOfType<Consul>();
        }

        [Fact]
        public void should_return_PollingConsulServiceDiscoveryProvider()
        {
            var stopsPollerFromPolling = 10000;
            var provider = ConsulProviderFactory.Get(_provider, 
                new ServiceProviderConfiguration("pollconsul", "","", 1, "", "", stopsPollerFromPolling), 
                JsonConvert.DeserializeObject<DownstreamRoute>("{}"));
            provider.ShouldBeOfType<PollConsul>();
        }
    }
}

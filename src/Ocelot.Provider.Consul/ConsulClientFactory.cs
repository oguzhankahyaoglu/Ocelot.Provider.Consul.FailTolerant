namespace Ocelot.Provider.Consul
{
    using System;
    using global::Consul;

    public class ConsulClientFactory : IConsulClientFactory
    {
        public IConsulClient Get(ConsulRegistryConfiguration config)
        {
            return new ConsulClient(c =>
            {
                var scheme = "http";
                if (!string.IsNullOrEmpty(config.Scheme))
                {
                    scheme = config.Scheme;
                }

                c.Address = new Uri($"{scheme}://{config.Host}:{config.Port}");

                if (!string.IsNullOrEmpty(config?.Token))
                {
                    c.Token = config.Token;
                }
            });
        }
    }
}

namespace Ocelot.Provider.Consul
{
    public class ConsulRegistryConfiguration
    {
        public ConsulRegistryConfiguration(  string scheme,
            string host, int port, string keyOfServiceInConsul, string token)
        {
            this.Host = string.IsNullOrEmpty(host) ? "localhost" : host;
            this.Port = port > 0 ? port : 8500;
            this.Scheme = string.IsNullOrEmpty(scheme) ? "http" : scheme;
            this.KeyOfServiceInConsul = keyOfServiceInConsul;
            this.Token = token;
        }

        public string Scheme { get; }
        public string KeyOfServiceInConsul { get; }
        public string Host { get; }
        public int Port { get; }
        public string Token { get; }
    }
}

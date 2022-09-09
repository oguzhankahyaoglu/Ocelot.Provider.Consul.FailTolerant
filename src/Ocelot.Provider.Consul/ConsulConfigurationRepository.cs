using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using Ocelot.Cache;
using Ocelot.Errors;
using Ocelot.Requester;

namespace Ocelot.Provider.Consul
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Configuration.File;
    using Configuration.Repository;
    using global::Consul;
    using Logging;
    using Newtonsoft.Json;
    using Responses;

    public class ConsulConfigurationRepository : IFileConfigurationRepository
    {
        private readonly IConsulClient _consul;
        private readonly string _configurationKey;
        private readonly IOcelotCache<FileConfiguration> _cache;
        private readonly IOcelotLogger _logger;
        public static DateTime? LastOcelotConfigFetchedSuccessfully { get; private set; }
        public static DateTime? LastOcelotConfigFetchTried { get; private set; }

        public ConsulConfigurationRepository(
            IOptions<FileConfiguration> fileConfiguration,
            IOcelotCache<FileConfiguration> cache,
            IConsulClientFactory factory,
            IOcelotLoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ConsulConfigurationRepository>();
            _cache = cache;
            var discoveryProvider =
                fileConfiguration.Value.GlobalConfiguration.ServiceDiscoveryProvider;
            _configurationKey = string.IsNullOrWhiteSpace(discoveryProvider.ConfigurationKey)
                ? "InternalConfiguration"
                : discoveryProvider.ConfigurationKey;
            var config = new ConsulRegistryConfiguration(discoveryProvider.Scheme,
                discoveryProvider.Host, discoveryProvider.Port, _configurationKey, discoveryProvider.Token);
            _consul = factory.Get(config);
        }

        public async Task<Response<FileConfiguration>> Get()
        {
            var data = _cache.Get(_configurationKey, _configurationKey);
            if (data != null)
            {
                return new OkResponse<FileConfiguration>(data);
            }

            try
            {
                LastOcelotConfigFetchTried = DateTime.Now;
                var queryResult = await _consul.KV.Get(_configurationKey);
                if (queryResult.Response == null)
                {
                    return new ErrorResponse<FileConfiguration>(new List<Error>
                    {
                        new RequestCanceledError(
                            $"Cannot get consul config successfully,statusCode:{queryResult.StatusCode}")
                    });
                }

                LastOcelotConfigFetchedSuccessfully = DateTime.Now;
                return new OkResponse<FileConfiguration>(
                    JsonConvert.DeserializeObject<FileConfiguration>(
                        Encoding.UTF8.GetString(queryResult.Response.Value)));
            }
            catch (HttpRequestException e)
            {
                _logger.LogError("Cannot fetch initial configuration from Consul.", e);
                return new ErrorResponse<FileConfiguration>(new List<Error>
                {
                    new RequestCanceledError(e.Message)
                });
            }     
            catch (SocketException e)
            {
                _logger.LogError("Cannot fetch initial configuration from Consul.", e);
                return new ErrorResponse<FileConfiguration>(new List<Error>
                {
                    new RequestCanceledError(e.Message)
                });
            }
        }

        public async Task<Response> Set(FileConfiguration ocelotConfiguration)
        {
            var bytes =
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ocelotConfiguration, Formatting.Indented));
            var writeResult = await _consul.KV.Put(new KVPair(_configurationKey)
            {
                Value = bytes
            });
            if (writeResult.Response)
            {
                _cache.AddAndDelete(_configurationKey, ocelotConfiguration, TimeSpan.FromSeconds(3.0),
                    _configurationKey);
                return new OkResponse();
            }

            var interpolatedStringHandler =
                $"Unable to set FileConfiguration in consul, response status code from consul was {writeResult.StatusCode}";
            return new ErrorResponse(
                new UnableToSetConfigInConsulError(interpolatedStringHandler, (int)writeResult.StatusCode));
        }
    }
}
